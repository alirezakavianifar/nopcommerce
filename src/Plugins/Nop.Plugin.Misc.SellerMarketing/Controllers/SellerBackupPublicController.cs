using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.SellerMarketing.Domain;
using Nop.Plugin.Misc.SellerMarketing.Models;
using Nop.Plugin.Misc.SellerMarketing.Services;
using Nop.Services.Configuration;
using Nop.Services.Messages;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.SellerMarketing.Controllers;

[AutoValidateAntiforgeryToken]
public class SellerBackupPublicController : BasePluginController
{
    private readonly ISellerBackupService _sellerBackupService;
    private readonly ISellerRestoreService _sellerRestoreService;
    private readonly ISettingService _settingService;
    private readonly IWorkContext _workContext;
    private readonly INotificationService _notificationService;

    public SellerBackupPublicController(
        ISellerBackupService sellerBackupService,
        ISellerRestoreService sellerRestoreService,
        ISettingService settingService,
        IWorkContext workContext,
        INotificationService notificationService)
    {
        _sellerBackupService = sellerBackupService;
        _sellerRestoreService = sellerRestoreService;
        _settingService = settingService;
        _workContext = workContext;
        _notificationService = notificationService;
    }

    protected virtual async Task<int> GetCurrentVendorIdAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        return customer?.VendorId ?? 0;
    }

    public virtual async Task<IActionResult> Index()
    {
        var vendorId = await GetCurrentVendorIdAsync();
        if (vendorId == 0)
            return RedirectToRoute("CustomerInfo");

        var settings = await _settingService.LoadSettingAsync<SellerBackupSettings>();
        var backups = await _sellerBackupService.GetBackupsByVendorIdAsync(vendorId);
        var restoreRequests = await _sellerRestoreService.GetAllRestoreRequestsAsync(vendorId);

        ViewBag.AllowExternalRestore = settings.AllowExternalFileRestore;
        ViewBag.Backups = backups.Select(b => new SellerBackupItemModel
        {
            Id = b.Id,
            FileName = b.FileName,
            IncludedEntities = b.IncludedEntities,
            FileSizeText = $"{b.FileSizeBytes / 1024.0:F1} KB",
            CreatedOn = b.CreatedOnUtc
        }).ToList();

        ViewBag.RestoreRequests = restoreRequests.Select(r => new SellerRestoreRequestModel
        {
            Id = r.Id,
            VendorId = r.VendorId,
            IsExternalUpload = r.IsExternalUpload,
            StatusName = r.Status.ToString(),
            AdminComment = r.AdminComment,
            RequestedOn = r.RequestedOnUtc,
            ReviewedOn = r.ReviewedOnUtc
        }).ToList();

        return View("~/Plugins/Misc.SellerMarketing/Views/SellerBackup/Index.cshtml");
    }

    [HttpPost]
    public virtual async Task<IActionResult> CreateBackup()
    {
        var vendorId = await GetCurrentVendorIdAsync();
        if (vendorId == 0)
            return RedirectToRoute("CustomerInfo");

        try
        {
            var record = await _sellerBackupService.CreateBackupAsync(vendorId);
            _notificationService.SuccessNotification("Backup created successfully on the server. You can now download a local copy.");
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification($"Failed to create backup: {ex.Message}");
        }

        return RedirectToAction("Index");
    }

    public virtual async Task<IActionResult> DownloadBackup(int id)
    {
        var vendorId = await GetCurrentVendorIdAsync();
        if (vendorId == 0)
            return RedirectToRoute("CustomerInfo");

        var backup = await _sellerBackupService.GetBackupByIdAsync(id);
        if (backup == null || backup.VendorId != vendorId)
            return NotFound();

        var stream = await _sellerBackupService.GetBackupFileStreamAsync(backup);
        return File(stream, "application/zip", backup.FileName);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RequestRestore(int backupId)
    {
        var vendorId = await GetCurrentVendorIdAsync();
        if (vendorId == 0)
            return RedirectToRoute("CustomerInfo");

        try
        {
            await _sellerRestoreService.CreateRestoreRequestFromExistingBackupAsync(vendorId, backupId);
            _notificationService.SuccessNotification("Restore request submitted successfully and is awaiting Administrator approval.");
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification($"Failed to submit restore request: {ex.Message}");
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public virtual async Task<IActionResult> UploadAndRequestRestore(IFormFile backupFile)
    {
        var vendorId = await GetCurrentVendorIdAsync();
        if (vendorId == 0)
            return RedirectToRoute("CustomerInfo");

        try
        {
            await _sellerRestoreService.CreateRestoreRequestFromUploadedFileAsync(vendorId, backupFile);
            _notificationService.SuccessNotification("External backup uploaded, cryptographically verified, and queued for Administrator review.");
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification($"External restore request failed: {ex.Message}");
        }

        return RedirectToAction("Index");
    }
}
