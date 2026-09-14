using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.SellerMarketing.Domain;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.SellerMarketing.Services;

public class SellerBackupService : ISellerBackupService
{
    private readonly IRepository<SellerBackupRecord> _backupRecordRepository;
    private readonly ISettingService _settingService;
    private readonly IProductService _productService;
    private readonly INopFileProvider _fileProvider;
    private readonly ILogger _logger;

    public SellerBackupService(
        IRepository<SellerBackupRecord> backupRecordRepository,
        ISettingService settingService,
        IProductService productService,
        INopFileProvider fileProvider,
        ILogger logger)
    {
        _backupRecordRepository = backupRecordRepository;
        _settingService = settingService;
        _productService = productService;
        _fileProvider = fileProvider;
        _logger = logger;
    }

    public virtual async Task<SellerBackupRecord> CreateBackupAsync(int vendorId)
    {
        if (vendorId <= 0)
            throw new ArgumentException("Invalid vendor identifier.", nameof(vendorId));

        var settings = await _settingService.LoadSettingAsync<SellerBackupSettings>();
        var includedEntities = new List<string>();

        var exportData = new Dictionary<string, object>();

        // 1. Export Products (if allowed)
        if (settings.AllowBackupProducts)
        {
            includedEntities.Add("Products");
            var products = await _productService.SearchProductsAsync(vendorId: vendorId, showHidden: true);
            var productList = new List<object>();

            foreach (var p in products)
            {
                productList.Add(new
                {
                    p.Id,
                    p.Name,
                    p.ShortDescription,
                    p.FullDescription,
                    p.Sku,
                    p.Price,
                    p.OldPrice,
                    p.ProductCost,
                    p.StockQuantity,
                    p.Published,
                    p.VisibleIndividually
                });
            }

            exportData["Products"] = productList;
        }

        // 2. Note on Orders / Financial data:
        // Explicitly barred per client requirements unless explicitly toggled by admin
        if (settings.AllowBackupOrders)
            includedEntities.Add("Orders");
        if (settings.AllowBackupFinancialData)
            includedEntities.Add("FinancialReports");

        var jsonData = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
        var dataBytes = Encoding.UTF8.GetBytes(jsonData);

        // Compute SHA-256 hash of the payload
        string dataHashSha256;
        using (var sha = SHA256.Create())
        {
            dataHashSha256 = Convert.ToHexString(sha.ComputeHash(dataBytes));
        }

        // Build Manifest
        var backupId = Guid.NewGuid().ToString("N");
        var createdAtUtc = DateTime.UtcNow;
        var includedEntitiesStr = string.Join(",", includedEntities);

        var manifestString = $"{backupId}|{vendorId}|{createdAtUtc:O}|{includedEntitiesStr}|{dataHashSha256}";

        // Compute HMAC-SHA256 signature using secret key
        string hmacSignature;
        var secretKeyBytes = Encoding.UTF8.GetBytes(settings.HmacSignatureSecretKey ?? "default_secret_key");
        using (var hmac = new HMACSHA256(secretKeyBytes))
        {
            hmacSignature = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(manifestString)));
        }

        var manifestObj = new
        {
            BackupId = backupId,
            VendorId = vendorId,
            CreatedAtUtc = createdAtUtc,
            IncludedEntities = includedEntitiesStr,
            DataHashSha256 = dataHashSha256,
            Signature = hmacSignature
        };

        var manifestJson = JsonSerializer.Serialize(manifestObj, new JsonSerializerOptions { WriteIndented = true });

        // Prepare local server directory: App_Data/SellerBackups/{vendorId}/
        var storageDir = _fileProvider.MapPath($"~/App_Data/{settings.BackupDirectoryPath}/{vendorId}");
        _fileProvider.CreateDirectory(storageDir);

        var fileName = $"backup_vendor_{vendorId}_{createdAtUtc:yyyyMMdd_HHmmss}_{backupId[..8]}.zip";
        var zipFilePath = Path.Combine(storageDir, fileName);

        // Create Zip Archive containing manifest.json and data.json
        using (var zipStream = new FileStream(zipFilePath, FileMode.Create, FileAccess.Write))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
        {
            // Add manifest.json
            var manifestEntry = archive.CreateEntry("manifest.json", CompressionLevel.Optimal);
            using (var entryStream = manifestEntry.Open())
            using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
            {
                await writer.WriteAsync(manifestJson);
            }

            // Add data.json
            var dataEntry = archive.CreateEntry("data.json", CompressionLevel.Optimal);
            using (var entryStream = dataEntry.Open())
            {
                await entryStream.WriteAsync(dataBytes);
            }
        }

        // Compute whole-file SHA-256 for storage tracking
        var fileBytes = await File.ReadAllBytesAsync(zipFilePath);
        string fileHashSha256;
        using (var sha = SHA256.Create())
        {
            fileHashSha256 = Convert.ToHexString(sha.ComputeHash(fileBytes));
        }

        var backupRecord = new SellerBackupRecord
        {
            VendorId = vendorId,
            FileName = fileName,
            FilePath = zipFilePath,
            FileHashSha256 = fileHashSha256,
            HmacSignature = hmacSignature,
            IncludedEntities = includedEntitiesStr,
            FileSizeBytes = fileBytes.Length,
            CreatedOnUtc = createdAtUtc
        };

        await _backupRecordRepository.InsertAsync(backupRecord);
        await _logger.InformationAsync($"Created secure backup for vendor {vendorId}, file: {fileName}");

        return backupRecord;
    }

    public virtual async Task<IList<SellerBackupRecord>> GetBackupsByVendorIdAsync(int vendorId)
    {
        return await _backupRecordRepository.GetAllAsync(query =>
            query.Where(b => b.VendorId == vendorId).OrderByDescending(b => b.CreatedOnUtc));
    }

    public virtual async Task<SellerBackupRecord> GetBackupByIdAsync(int id)
    {
        return await _backupRecordRepository.GetByIdAsync(id);
    }

    public virtual Task<Stream> GetBackupFileStreamAsync(SellerBackupRecord record)
    {
        if (record == null || !File.Exists(record.FilePath))
            throw new FileNotFoundException("Backup archive file not found on server.");

        Stream stream = new FileStream(record.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public virtual async Task DeleteBackupAsync(SellerBackupRecord record)
    {
        if (record == null)
            return;

        if (File.Exists(record.FilePath))
        {
            try
            {
                File.Delete(record.FilePath);
            }
            catch (Exception ex)
            {
                await _logger.WarningAsync($"Could not delete backup archive file: {record.FilePath}. Error: {ex.Message}");
            }
        }

        await _backupRecordRepository.DeleteAsync(record);
    }
}
