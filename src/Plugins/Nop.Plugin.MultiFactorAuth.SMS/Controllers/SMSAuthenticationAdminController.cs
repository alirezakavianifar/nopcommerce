using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.MultiFactorAuth.SMS.Domains;
using Nop.Plugin.MultiFactorAuth.SMS.Models;
using Nop.Plugin.MultiFactorAuth.SMS.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.MultiFactorAuth.SMS.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class SMSAuthenticationAdminController : BasePluginController
{
    protected readonly ISettingService _settingService;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IPermissionService _permissionService;
    protected readonly ICustomerSecurityRestrictionService _securityRestrictionService;

    public SMSAuthenticationAdminController(
        ISettingService settingService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ICustomerSecurityRestrictionService securityRestrictionService)
    {
        _settingService = settingService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _securityRestrictionService = securityRestrictionService;
    }

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_MULTIFACTOR_AUTHENTICATION_METHODS))
            return AccessDeniedView();

        var settings = await _settingService.LoadSettingAsync<SMSSettings>();
        var model = new ConfigurationModel
        {
            Provider = settings.Provider,
            ApiKey = settings.ApiKey,
            ApiSecret = settings.ApiSecret,
            SenderNumber = settings.SenderNumber,
            CodeLength = settings.CodeLength,
            CodeLifetimeMinutes = settings.CodeLifetimeMinutes,
            Force2FAForAdmins = settings.Force2FAForAdmins,
            Force2FAForVendors = settings.Force2FAForVendors,
            EnableIpAllowlist = settings.EnableIpAllowlist,
            EnableDeviceBinding = settings.EnableDeviceBinding,
            ForceDeviceBindingForAdmins = settings.ForceDeviceBindingForAdmins,
            ForceDeviceBindingForVendors = settings.ForceDeviceBindingForVendors
        };

        return View("~/Plugins/MultiFactorAuth.SMS/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_MULTIFACTOR_AUTHENTICATION_METHODS))
            return AccessDeniedView();

        if (!ModelState.IsValid)
            return await Configure();

        var settings = await _settingService.LoadSettingAsync<SMSSettings>();
        settings.Provider = model.Provider;
        settings.ApiKey = model.ApiKey;
        settings.ApiSecret = model.ApiSecret;
        settings.SenderNumber = model.SenderNumber;
        settings.CodeLength = model.CodeLength > 0 ? model.CodeLength : 6;
        settings.CodeLifetimeMinutes = model.CodeLifetimeMinutes > 0 ? model.CodeLifetimeMinutes : 3;
        settings.Force2FAForAdmins = model.Force2FAForAdmins;
        settings.Force2FAForVendors = model.Force2FAForVendors;
        settings.EnableIpAllowlist = model.EnableIpAllowlist;
        settings.EnableDeviceBinding = model.EnableDeviceBinding;
        settings.ForceDeviceBindingForAdmins = model.ForceDeviceBindingForAdmins;
        settings.ForceDeviceBindingForVendors = model.ForceDeviceBindingForVendors;

        await _settingService.SaveSettingAsync(settings);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    [HttpPost]
    public async Task<IActionResult> SaveCustomerRestrictions(int customerId, string allowedIps)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_MULTIFACTOR_AUTHENTICATION_METHODS))
            return Json(new { success = false, message = "Access denied." });

        if (customerId == 0)
            return Json(new { success = false, message = "Invalid customer identifier." });

        var restrictions = await _securityRestrictionService.GetSecurityRestrictionsByCustomerIdAsync(customerId);
        var ipRecord = restrictions.FirstOrDefault(r => string.IsNullOrEmpty(r.DeviceTokenHash));

        if (ipRecord != null)
        {
            if (string.IsNullOrWhiteSpace(allowedIps))
            {
                await _securityRestrictionService.DeleteSecurityRestrictionAsync(ipRecord);
            }
            else
            {
                ipRecord.AllowedIpAddresses = allowedIps;
                await _securityRestrictionService.SaveSecurityRestrictionAsync(ipRecord);
            }
        }
        else if (!string.IsNullOrWhiteSpace(allowedIps))
        {
            ipRecord = new CustomerSecurityRestriction
            {
                CustomerId = customerId,
                AllowedIpAddresses = allowedIps,
                IsApproved = true,
                CreatedOnUtc = DateTime.UtcNow
            };
            await _securityRestrictionService.SaveSecurityRestrictionAsync(ipRecord);
        }

        return Json(new { success = true, message = "Allowed IP configuration saved successfully." });
    }

    [HttpPost]
    public async Task<IActionResult> RevokeDevice(int restrictionId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_MULTIFACTOR_AUTHENTICATION_METHODS))
            return Json(new { success = false, message = "Access denied." });

        var record = await _securityRestrictionService.GetSecurityRestrictionByIdAsync(restrictionId);
        if (record != null)
        {
            await _securityRestrictionService.DeleteSecurityRestrictionAsync(record);
            return Json(new { success = true, message = "Device revoked successfully." });
        }

        return Json(new { success = false, message = "Device binding not found." });
    }

    [HttpPost]
    public async Task<IActionResult> ApproveDevice(int restrictionId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_MULTIFACTOR_AUTHENTICATION_METHODS))
            return Json(new { success = false, message = "Access denied." });

        var record = await _securityRestrictionService.GetSecurityRestrictionByIdAsync(restrictionId);
        if (record != null)
        {
            record.IsApproved = true;
            await _securityRestrictionService.SaveSecurityRestrictionAsync(record);
            return Json(new { success = true, message = "Device trusted successfully." });
        }

        return Json(new { success = false, message = "Device binding not found." });
    }
}
