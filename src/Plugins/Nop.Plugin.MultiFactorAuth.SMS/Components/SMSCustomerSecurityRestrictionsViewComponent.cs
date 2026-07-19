using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.MultiFactorAuth.SMS.Services;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.MultiFactorAuth.SMS.Components;

public class SMSCustomerSecurityRestrictionsViewComponent : NopViewComponent
{
    protected readonly ICustomerSecurityRestrictionService _securityRestrictionService;

    public SMSCustomerSecurityRestrictionsViewComponent(ICustomerSecurityRestrictionService securityRestrictionService)
    {
        _securityRestrictionService = securityRestrictionService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var customerId = 0;
        
        if (additionalData != null)
        {
            // Use reflection to extract Id from additionalData to avoid circular reference to Nop.Web.Areas.Admin
            var idProperty = additionalData.GetType().GetProperty("Id");
            if (idProperty != null)
            {
                var value = idProperty.GetValue(additionalData);
                if (value is int intId)
                {
                    customerId = intId;
                }
            }
        }

        if (customerId == 0)
            return Content(string.Empty);

        var restrictions = await _securityRestrictionService.GetSecurityRestrictionsByCustomerIdAsync(customerId);
        
        // Find IP restriction (represented by record with empty device hash)
        var ipRecord = restrictions.FirstOrDefault(r => string.IsNullOrEmpty(r.DeviceTokenHash));
        var allowedIps = ipRecord?.AllowedIpAddresses ?? string.Empty;

        // Find device bindings
        var devices = restrictions.Where(r => !string.IsNullOrEmpty(r.DeviceTokenHash)).ToList();

        ViewBag.CustomerId = customerId;
        ViewBag.AllowedIps = allowedIps;

        return View("~/Plugins/MultiFactorAuth.SMS/Views/SMSCustomerSecurityRestrictions.cshtml", devices);
    }
}
