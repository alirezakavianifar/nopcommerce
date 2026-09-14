using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.SellerMarketing.Domain;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.SellerMarketing.Services;

public class SellerRestoreService : ISellerRestoreService
{
    private readonly IRepository<SellerRestoreRequest> _restoreRequestRepository;
    private readonly IRepository<SellerBackupRecord> _backupRecordRepository;
    private readonly ISettingService _settingService;
    private readonly IAntivirusScanner _antivirusScanner;
    private readonly IProductService _productService;
    private readonly INopFileProvider _fileProvider;
    private readonly ILogger _logger;

    public SellerRestoreService(
        IRepository<SellerRestoreRequest> restoreRequestRepository,
        IRepository<SellerBackupRecord> backupRecordRepository,
        ISettingService settingService,
        IAntivirusScanner antivirusScanner,
        IProductService productService,
        INopFileProvider fileProvider,
        ILogger logger)
    {
        _restoreRequestRepository = restoreRequestRepository;
        _backupRecordRepository = backupRecordRepository;
        _settingService = settingService;
        _antivirusScanner = antivirusScanner;
        _productService = productService;
        _fileProvider = fileProvider;
        _logger = logger;
    }

    public virtual async Task<SellerRestoreRequest> CreateRestoreRequestFromExistingBackupAsync(int vendorId, int backupRecordId)
    {
        var backup = await _backupRecordRepository.GetByIdAsync(backupRecordId);
        if (backup == null || backup.VendorId != vendorId)
            throw new ArgumentException("Backup record not found or does not belong to this vendor.");

        if (!File.Exists(backup.FilePath))
            throw new FileNotFoundException("Backup archive file is missing on the server.");

        // Anti-tamper verification of the server file
        var fileBytes = await File.ReadAllBytesAsync(backup.FilePath);
        string currentHash;
        using (var sha = SHA256.Create())
        {
            currentHash = Convert.ToHexString(sha.ComputeHash(fileBytes));
        }

        if (!string.Equals(currentHash, backup.FileHashSha256, StringComparison.OrdinalIgnoreCase))
        {
            await _logger.ErrorAsync($"Integrity verification failed for backup {backupRecordId}. File hash mismatch.");
            throw new InvalidOperationException("Backup archive integrity verification failed. The file on server has been modified or corrupted.");
        }

        var request = new SellerRestoreRequest
        {
            VendorId = vendorId,
            BackupRecordId = backupRecordId,
            IsExternalUpload = false,
            StagedFilePath = backup.FilePath,
            FileHashSha256 = currentHash,
            StatusId = (int)RestoreRequestStatus.PendingApproval,
            RequestedOnUtc = DateTime.UtcNow
        };

        await _restoreRequestRepository.InsertAsync(request);
        await _logger.InformationAsync($"Vendor {vendorId} submitted restore request for server backup {backupRecordId}. Awaiting admin approval.");

        return request;
    }

    public virtual async Task<SellerRestoreRequest> CreateRestoreRequestFromUploadedFileAsync(int vendorId, IFormFile file)
    {
        if (vendorId <= 0)
            throw new ArgumentException("Invalid vendor identifier.", nameof(vendorId));

        if (file == null || file.Length == 0)
            throw new ArgumentException("No file uploaded.", nameof(file));

        var settings = await _settingService.LoadSettingAsync<SellerBackupSettings>();
        if (!settings.AllowExternalFileRestore)
        {
            throw new InvalidOperationException("External backup file restoration is disabled by administrator policy. Only server-stored backups can be restored.");
        }

        // 1. Stage in quarantine folder
        var quarantineDir = _fileProvider.MapPath($"~/App_Data/{settings.BackupDirectoryPath}/Quarantine/{vendorId}");
        _fileProvider.CreateDirectory(quarantineDir);

        var stagedFileName = $"staged_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.zip";
        var stagedFilePath = Path.Combine(quarantineDir, stagedFileName);

        await using (var stream = new FileStream(stagedFilePath, FileMode.Create, FileAccess.Write))
        {
            await file.CopyToAsync(stream);
        }

        // 2. Antivirus & zip security scan
        var (isClean, errorReason) = await _antivirusScanner.ScanArchiveAsync(stagedFilePath);
        if (!isClean)
        {
            try { File.Delete(stagedFilePath); } catch { }
            throw new InvalidOperationException($"Security validation failed: {errorReason}");
        }

        // 3. Extract and verify manifest.json and HMAC signature (Anti-Tampering)
        string manifestContent = null;
        byte[] dataBytes = null;

        using (var archive = ZipFile.OpenRead(stagedFilePath))
        {
            var manifestEntry = archive.GetEntry("manifest.json");
            var dataEntry = archive.GetEntry("data.json");

            if (manifestEntry == null || dataEntry == null)
            {
                try { File.Delete(stagedFilePath); } catch { }
                throw new InvalidOperationException("Invalid backup package format. Required manifest or data payloads are missing.");
            }

            using (var reader = new StreamReader(manifestEntry.Open(), Encoding.UTF8))
            {
                manifestContent = await reader.ReadToEndAsync();
            }

            using (var memoryStream = new MemoryStream())
            {
                await using (var entryStream = dataEntry.Open())
                {
                    await entryStream.CopyToAsync(memoryStream);
                }
                dataBytes = memoryStream.ToArray();
            }
        }

        using var manifestDoc = JsonDocument.Parse(manifestContent);
        var root = manifestDoc.RootElement;

        var manifestBackupId = root.GetProperty("BackupId").GetString();
        var manifestVendorId = root.GetProperty("VendorId").GetInt32();
        var manifestCreatedAt = root.GetProperty("CreatedAtUtc").GetDateTime();
        var manifestEntities = root.GetProperty("IncludedEntities").GetString();
        var manifestDataHash = root.GetProperty("DataHashSha256").GetString();
        var manifestSignature = root.GetProperty("Signature").GetString();

        // Check that backup actually belongs to this vendor
        if (manifestVendorId != vendorId)
        {
            try { File.Delete(stagedFilePath); } catch { }
            throw new InvalidOperationException("Unauthorized: This backup archive belongs to another vendor account.");
        }

        // Verify data payload hash
        string actualDataHash;
        using (var sha = SHA256.Create())
        {
            actualDataHash = Convert.ToHexString(sha.ComputeHash(dataBytes));
        }

        if (!string.Equals(actualDataHash, manifestDataHash, StringComparison.OrdinalIgnoreCase))
        {
            try { File.Delete(stagedFilePath); } catch { }
            throw new InvalidOperationException("Anti-tamper violation: Backup data payload checksum does not match manifest.");
        }

        // Verify HMAC signature with server secret key
        var expectedManifestString = $"{manifestBackupId}|{manifestVendorId}|{manifestCreatedAt:O}|{manifestEntities}|{manifestDataHash}";
        var secretKeyBytes = Encoding.UTF8.GetBytes(settings.HmacSignatureSecretKey ?? "default_secret_key");
        string expectedSignature;
        using (var hmac = new HMACSHA256(secretKeyBytes))
        {
            expectedSignature = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(expectedManifestString)));
        }

        if (!string.Equals(expectedSignature, manifestSignature, StringComparison.OrdinalIgnoreCase))
        {
            try { File.Delete(stagedFilePath); } catch { }
            await _logger.WarningAsync($"Tampered backup upload attempt detected for vendor {vendorId}. Invalid digital signature.");
            throw new InvalidOperationException("Cryptographic verification failed: The backup file has been altered or was not generated by this system.");
        }

        // Compute whole file SHA-256
        var fileBytes = await File.ReadAllBytesAsync(stagedFilePath);
        string fileHashSha256;
        using (var sha = SHA256.Create())
        {
            fileHashSha256 = Convert.ToHexString(sha.ComputeHash(fileBytes));
        }

        var request = new SellerRestoreRequest
        {
            VendorId = vendorId,
            BackupRecordId = null,
            IsExternalUpload = true,
            StagedFilePath = stagedFilePath,
            FileHashSha256 = fileHashSha256,
            StatusId = (int)RestoreRequestStatus.PendingApproval,
            RequestedOnUtc = DateTime.UtcNow
        };

        await _restoreRequestRepository.InsertAsync(request);
        await _logger.InformationAsync($"Vendor {vendorId} uploaded external backup for restore request. Passed all security and signature checks. Awaiting admin approval.");

        return request;
    }

    public virtual async Task<bool> ApproveAndExecuteRestoreAsync(int requestId, string adminComment)
    {
        var request = await _restoreRequestRepository.GetByIdAsync(requestId);
        if (request == null || request.Status != RestoreRequestStatus.PendingApproval)
            return false;

        request.StatusId = (int)RestoreRequestStatus.Executing;
        request.AdminComment = adminComment;
        request.ReviewedOnUtc = DateTime.UtcNow;
        await _restoreRequestRepository.UpdateAsync(request);

        try
        {
            if (!File.Exists(request.StagedFilePath))
                throw new FileNotFoundException("Staged archive file is missing.");

            byte[] dataBytes;
            using (var archive = ZipFile.OpenRead(request.StagedFilePath))
            {
                var dataEntry = archive.GetEntry("data.json");
                if (dataEntry == null)
                    throw new InvalidOperationException("data.json is missing in archive.");

                using var memoryStream = new MemoryStream();
                await using (var stream = dataEntry.Open())
                {
                    await stream.CopyToAsync(memoryStream);
                }
                dataBytes = memoryStream.ToArray();
            }

            var jsonString = Encoding.UTF8.GetString(dataBytes);
            using var jsonDoc = JsonDocument.Parse(jsonString);

            if (jsonDoc.RootElement.TryGetProperty("Products", out var productsElement))
            {
                foreach (var productObj in productsElement.EnumerateArray())
                {
                    var sku = productObj.TryGetProperty("Sku", out var skuProp) ? skuProp.GetString() : string.Empty;
                    var name = productObj.TryGetProperty("Name", out var nameProp) ? nameProp.GetString() : string.Empty;
                    var price = productObj.TryGetProperty("Price", out var priceProp) ? priceProp.GetDecimal() : 0m;
                    var cost = productObj.TryGetProperty("ProductCost", out var costProp) ? costProp.GetDecimal() : 0m;
                    var stock = productObj.TryGetProperty("StockQuantity", out var stockProp) ? stockProp.GetInt32() : 0;
                    var shortDesc = productObj.TryGetProperty("ShortDescription", out var shortDescProp) ? shortDescProp.GetString() : string.Empty;
                    var fullDesc = productObj.TryGetProperty("FullDescription", out var fullDescProp) ? fullDescProp.GetString() : string.Empty;

                    // Match existing product by SKU for this vendor
                    Product existingProduct = null;
                    if (!string.IsNullOrEmpty(sku))
                    {
                        var vendorProducts = await _productService.SearchProductsAsync(vendorId: request.VendorId, showHidden: true);
                        existingProduct = vendorProducts.FirstOrDefault(p => string.Equals(p.Sku, sku, StringComparison.OrdinalIgnoreCase));
                    }

                    if (existingProduct != null)
                    {
                        // Ensure product strictly belongs to this vendor
                        if (existingProduct.VendorId == request.VendorId)
                        {
                            existingProduct.Name = name;
                            existingProduct.Price = price;
                            existingProduct.ProductCost = cost;
                            existingProduct.StockQuantity = stock;
                            existingProduct.ShortDescription = shortDesc;
                            existingProduct.FullDescription = fullDesc;
                            existingProduct.UpdatedOnUtc = DateTime.UtcNow;
                            await _productService.UpdateProductAsync(existingProduct);
                        }
                    }
                    else if (!string.IsNullOrEmpty(name))
                    {
                        var newProduct = new Product
                        {
                            VendorId = request.VendorId,
                            Name = name,
                            Sku = sku,
                            Price = price,
                            ProductCost = cost,
                            StockQuantity = stock,
                            ShortDescription = shortDesc,
                            FullDescription = fullDesc,
                            Published = true,
                            CreatedOnUtc = DateTime.UtcNow,
                            UpdatedOnUtc = DateTime.UtcNow
                        };
                        await _productService.InsertProductAsync(newProduct);
                    }
                }
            }

            request.StatusId = (int)RestoreRequestStatus.Completed;
            await _restoreRequestRepository.UpdateAsync(request);
            await _logger.InformationAsync($"Restore request {requestId} for vendor {request.VendorId} successfully executed.");
            return true;
        }
        catch (Exception ex)
        {
            request.StatusId = (int)RestoreRequestStatus.Failed;
            request.AdminComment += $" | Execution Error: {ex.Message}";
            await _restoreRequestRepository.UpdateAsync(request);
            await _logger.ErrorAsync($"Failed to execute restore request {requestId}: {ex.Message}", ex);
            return false;
        }
    }

    public virtual async Task<bool> RejectRestoreAsync(int requestId, string adminComment)
    {
        var request = await _restoreRequestRepository.GetByIdAsync(requestId);
        if (request == null || request.Status != RestoreRequestStatus.PendingApproval)
            return false;

        request.StatusId = (int)RestoreRequestStatus.Rejected;
        request.AdminComment = adminComment;
        request.ReviewedOnUtc = DateTime.UtcNow;
        await _restoreRequestRepository.UpdateAsync(request);

        await _logger.InformationAsync($"Restore request {requestId} for vendor {request.VendorId} rejected by admin. Comment: {adminComment}");
        return true;
    }

    public virtual async Task<IPagedList<SellerRestoreRequest>> GetAllRestoreRequestsAsync(int? vendorId = null, RestoreRequestStatus? status = null, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = _restoreRequestRepository.Table;
        if (vendorId.HasValue && vendorId.Value > 0)
        {
            query = query.Where(r => r.VendorId == vendorId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(r => r.StatusId == (int)status.Value);
        }

        query = query.OrderByDescending(r => r.RequestedOnUtc);
        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public virtual async Task<SellerRestoreRequest> GetRestoreRequestByIdAsync(int id)
    {
        return await _restoreRequestRepository.GetByIdAsync(id);
    }
}
