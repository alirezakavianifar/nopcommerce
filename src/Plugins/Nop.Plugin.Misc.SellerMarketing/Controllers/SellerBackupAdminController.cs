using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.SellerMarketing.Domain;
using Nop.Plugin.Misc.SellerMarketing.Models;
using Nop.Plugin.Misc.SellerMarketing.Services;
using Nop.Services.Configuration;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.SellerMarketing.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class SellerBackupAdminController : BasePluginController
{
    private readonly ISettingService _settingService;
    private readonly ISellerRestoreService _sellerRestoreService;
    private readonly IVendorService _vendorService;
    private readonly INotificationService _notificationService;

    public SellerBackupAdminController(
        ISettingService settingService,
        ISellerRestoreService sellerRestoreService,
        IVendorService vendorService,
        INotificationService notificationService)
    {
        _settingService = settingService;
        _sellerRestoreService = sellerRestoreService;
        _vendorService = vendorService;
        _notificationService = notificationService;
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> Configure()
    {
        var settings = await _settingService.LoadSettingAsync<SellerBackupSettings>();

        var model = new SellerBackupSettingsModel
        {
            AllowBackupProducts = settings.AllowBackupProducts,
            AllowBackupProductAttributes = settings.AllowBackupProductAttributes,
            AllowBackupPictures = settings.AllowBackupPictures,
            AllowBackupOrders = settings.AllowBackupOrders,
            AllowBackupFinancialData = settings.AllowBackupFinancialData,
            AllowExternalFileRestore = settings.AllowExternalFileRestore,
            RequireAdminApprovalForRestore = settings.RequireAdminApprovalForRestore,
            HmacSignatureSecretKey = settings.HmacSignatureSecretKey
        };

        return View("~/Plugins/Misc.SellerMarketing/Views/SellerBackupAdmin/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> Configure(SellerBackupSettingsModel model)
    {
        if (ModelState.IsValid)
        {
            var settings = await _settingService.LoadSettingAsync<SellerBackupSettings>();
            settings.AllowBackupProducts = model.AllowBackupProducts;
            settings.AllowBackupProductAttributes = model.AllowBackupProductAttributes;
            settings.AllowBackupPictures = model.AllowBackupPictures;
            settings.AllowBackupOrders = model.AllowBackupOrders;
            settings.AllowBackupFinancialData = model.AllowBackupFinancialData;
            settings.AllowExternalFileRestore = model.AllowExternalFileRestore;
            settings.RequireAdminApprovalForRestore = model.RequireAdminApprovalForRestore;
            settings.HmacSignatureSecretKey = string.IsNullOrWhiteSpace(model.HmacSignatureSecretKey)
                ? Guid.NewGuid().ToString("N")
                : model.HmacSignatureSecretKey;

            await _settingService.SaveSettingAsync(settings);
            _notificationService.SuccessNotification("Seller backup settings saved successfully.");

            return RedirectToAction("Configure");
        }

        return View("~/Plugins/Misc.SellerMarketing/Views/SellerBackupAdmin/Configure.cshtml", model);
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual IActionResult RestoreRequests()
    {
        var model = new SellerRestoreRequestSearchModel();
        return View("~/Plugins/Misc.SellerMarketing/Views/SellerBackupAdmin/RestoreRequests.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> RestoreRequestsData(SellerRestoreRequestSearchModel searchModel)
    {
        RestoreRequestStatus? status = searchModel.StatusId.HasValue ? (RestoreRequestStatus)searchModel.StatusId.Value : null;
        var requests = await _sellerRestoreService.GetAllRestoreRequestsAsync(searchModel.VendorId, status, searchModel.Page - 1, searchModel.PageSize);

        var model = await new SellerRestoreRequestListModel().PrepareToGridAsync(searchModel, requests, () =>
        {
            return requests.SelectAwait(async r =>
            {
                var vendor = await _vendorService.GetVendorByIdAsync(r.VendorId);
                return new SellerRestoreRequestModel
                {
                    Id = r.Id,
                    VendorId = r.VendorId,
                    VendorName = vendor?.Name ?? $"Vendor #{r.VendorId}",
                    IsExternalUpload = r.IsExternalUpload,
                    FileHashSha256 = r.FileHashSha256,
                    StatusName = r.Status.ToString(),
                    AdminComment = r.AdminComment,
                    RequestedOn = r.RequestedOnUtc,
                    ReviewedOn = r.ReviewedOnUtc
                };
            });
        });

        return Json(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> Approve(int id, string adminComment)
    {
        var success = await _sellerRestoreService.ApproveAndExecuteRestoreAsync(id, adminComment ?? "Approved by administrator.");
        if (success)
            _notificationService.SuccessNotification("Restore request approved and data successfully restored.");
        else
            _notificationService.ErrorNotification("Failed to execute restore request. Check the system log for details.");

        return RedirectToAction("RestoreRequests");
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> Reject(int id, string adminComment)
    {
        var success = await _sellerRestoreService.RejectRestoreAsync(id, adminComment ?? "Rejected by administrator.");
        if (success)
            _notificationService.SuccessNotification("Restore request rejected.");
        else
            _notificationService.ErrorNotification("Could not reject restore request.");

        return RedirectToAction("RestoreRequests");
    }
}
