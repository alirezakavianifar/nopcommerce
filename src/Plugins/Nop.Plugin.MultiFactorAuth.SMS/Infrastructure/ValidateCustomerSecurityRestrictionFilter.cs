using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.MultiFactorAuth.SMS.Services;
using Nop.Services.Customers;
using Nop.Web.Framework;

namespace Nop.Plugin.MultiFactorAuth.SMS.Infrastructure;

public class ValidateCustomerSecurityRestrictionFilter : IAsyncActionFilter
{
    protected readonly IWorkContext _workContext;
    protected readonly IWebHelper _webHelper;
    protected readonly ICustomerSecurityRestrictionService _securityRestrictionService;
    protected readonly SMSSettings _smsSettings;
    protected readonly ICustomerService _customerService;

    public ValidateCustomerSecurityRestrictionFilter(
        IWorkContext workContext,
        IWebHelper webHelper,
        ICustomerSecurityRestrictionService securityRestrictionService,
        SMSSettings smsSettings,
        ICustomerService customerService)
    {
        _workContext = workContext;
        _webHelper = webHelper;
        _securityRestrictionService = securityRestrictionService;
        _smsSettings = smsSettings;
        _customerService = customerService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.HttpContext.Request == null)
        {
            await next();
            return;
        }

        // Get action and controller names
        var controllerName = context.ActionDescriptor.RouteValues["controller"];
        var actionName = context.ActionDescriptor.RouteValues["action"];

        // Exclude the SMSAuthentication controller to prevent redirection loops
        if (string.Equals(controllerName, "SMSAuthentication", StringComparison.InvariantCultureIgnoreCase) ||
            string.Equals(controllerName, "SMSAuthenticationAdmin", StringComparison.InvariantCultureIgnoreCase))
        {
            await next();
            return;
        }

        // Get current customer
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || !await _customerService.IsRegisteredAsync(customer))
        {
            await next();
            return;
        }

        // Check if current area is Admin or Vendor (where high security restrictions are enforced)
        var area = context.RouteData.Values["area"]?.ToString();
        var isAdminOrVendorArea = string.Equals(area, "Admin", StringComparison.InvariantCultureIgnoreCase) ||
                                  string.Equals(area, "Vendor", StringComparison.InvariantCultureIgnoreCase);

        if (!isAdminOrVendorArea)
        {
            await next();
            return;
        }

        // 1. Check IP allowlist
        if (_smsSettings.EnableIpAllowlist)
        {
            var currentIp = _webHelper.GetCurrentIpAddress();
            var isAllowed = await _securityRestrictionService.IsIpAddressAllowedAsync(customer.Id, currentIp);
            if (!isAllowed)
            {
                // Redirect to Access Denied
                context.Result = new RedirectToActionResult("AccessDenied", "Security", new { area = "" });
                return;
            }
        }

        // 2. Check Device Binding
        var isDeviceBindingRequired = _smsSettings.EnableDeviceBinding;
        if (!isDeviceBindingRequired)
        {
            if (_smsSettings.ForceDeviceBindingForAdmins && await _customerService.IsAdminAsync(customer))
                isDeviceBindingRequired = true;
            else if (_smsSettings.ForceDeviceBindingForVendors && await _customerService.IsVendorAsync(customer))
                isDeviceBindingRequired = true;
        }

        if (isDeviceBindingRequired)
        {
            var deviceCookie = context.HttpContext.Request.Cookies["SMS2FA.DeviceToken"];
            var isDeviceValid = await _securityRestrictionService.IsDeviceTokenValidAsync(customer.Id, deviceCookie);

            if (!isDeviceValid)
            {
                // Device not registered or not approved. Redirect to device verification challenge
                context.Result = new RedirectToActionResult("DeviceVerificationChallenge", "SMSAuthentication", new { area = "" });
                return;
            }
        }

        await next();
    }
}
