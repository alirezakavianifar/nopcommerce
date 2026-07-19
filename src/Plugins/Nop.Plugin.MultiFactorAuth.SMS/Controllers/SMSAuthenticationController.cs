using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Http;
using Nop.Core.Http.Extensions;
using Nop.Plugin.MultiFactorAuth.SMS.Models;
using Nop.Plugin.MultiFactorAuth.SMS.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.MultiFactorAuth.SMS.Controllers;

[AutoValidateAntiforgeryToken]
public class SMSAuthenticationController : BasePluginController
{
    protected readonly ISMSService _smsService;
    protected readonly ICustomerService _customerService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly ICustomerRegistrationService _customerRegistrationService;
    protected readonly ICustomerSecurityRestrictionService _securityRestrictionService;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IWorkContext _workContext;
    protected readonly CustomerSettings _customerSettings;

    public SMSAuthenticationController(
        ISMSService smsService,
        ICustomerService customerService,
        IGenericAttributeService genericAttributeService,
        ICustomerRegistrationService customerRegistrationService,
        ICustomerSecurityRestrictionService securityRestrictionService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IWorkContext workContext,
        CustomerSettings customerSettings)
    {
        _smsService = smsService;
        _customerService = customerService;
        _genericAttributeService = genericAttributeService;
        _customerRegistrationService = customerRegistrationService;
        _securityRestrictionService = securityRestrictionService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _workContext = workContext;
        _customerSettings = customerSettings;
    }

    [HttpPost]
    public async Task<IActionResult> RequestEnrollmentCode(string phoneNumber)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        if (currentCustomer == null)
            return Json(new { success = false, message = "User not authenticated." });

        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Json(new { success = false, message = "Phone number is required." });

        try
        {
            var plainCodeForTesting = await _smsService.GenerateAndSendCodeAsync(currentCustomer.Email ?? currentCustomer.Username, phoneNumber);
            return Json(new { success = true, message = "Code sent successfully.", code = plainCodeForTesting });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Failed to send code: {ex.Message}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> RegisterSMS2FA(SMSAuthModel model)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        if (currentCustomer == null)
            return RedirectToRoute(NopRouteNames.General.LOGIN);

        if (string.IsNullOrWhiteSpace(model.PhoneNumber) || string.IsNullOrWhiteSpace(model.Code))
        {
            _notificationService.ErrorNotification("Phone number and validation code are required.");
            return RedirectToRoute(NopRouteNames.Standard.MULTI_FACTOR_AUTHENTICATION_SETTINGS);
        }

        var isValid = await _smsService.ValidateCodeAsync(currentCustomer.Email ?? currentCustomer.Username, model.Code);
        if (isValid)
        {
            await _genericAttributeService.SaveAttributeAsync(currentCustomer, SMSDefaults.SMS2FAPhoneNumberAttribute, model.PhoneNumber);
            await _genericAttributeService.SaveAttributeAsync(currentCustomer, SMSDefaults.SMS2FAEnabledAttribute, true);

            currentCustomer.Phone = model.PhoneNumber;
            await _customerService.UpdateCustomerAsync(currentCustomer);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.MultiFactorAuth.SMS.Customer.VerificationSuccess"));
        }
        else
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.MultiFactorAuth.SMS.Customer.VerificationFailed"));
        }

        return RedirectToRoute(NopRouteNames.Standard.MULTI_FACTOR_AUTHENTICATION_SETTINGS);
    }

    [HttpPost]
    public async Task<IActionResult> DisableSMS2FA()
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        if (currentCustomer == null)
            return RedirectToRoute(NopRouteNames.General.LOGIN);

        await _genericAttributeService.SaveAttributeAsync(currentCustomer, SMSDefaults.SMS2FAEnabledAttribute, false);
        _notificationService.SuccessNotification("SMS Two-Factor Authentication has been disabled.");

        return RedirectToRoute(NopRouteNames.Standard.MULTI_FACTOR_AUTHENTICATION_SETTINGS);
    }

    [HttpPost]
    public async Task<IActionResult> VerifySMS2FA(SMSTokenModel model)
    {
        var sessionInfo = await HttpContext.Session.GetAsync<CustomerMultiFactorAuthenticationInfo>(NopCustomerDefaults.CustomerMultiFactorAuthenticationInfo);
        if (sessionInfo == null || string.IsNullOrEmpty(sessionInfo.UserName))
            return RedirectToRoute(NopRouteNames.General.LOGIN);

        var customer = _customerSettings.UsernamesEnabled
            ? await _customerService.GetCustomerByUsernameAsync(sessionInfo.UserName)
            : await _customerService.GetCustomerByEmailAsync(sessionInfo.UserName);

        if (customer == null)
            return RedirectToRoute(NopRouteNames.General.LOGIN);

        var isValid = await _smsService.ValidateCodeAsync(customer.Email ?? customer.Username, model.Token);
        if (isValid)
        {
            await HttpContext.Session.SetAsync<CustomerMultiFactorAuthenticationInfo>(NopCustomerDefaults.CustomerMultiFactorAuthenticationInfo, null);
            return await _customerRegistrationService.SignInCustomerAsync(customer, sessionInfo.ReturnUrl, sessionInfo.RememberMe);
        }

        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.MultiFactorAuth.SMS.Customer.VerificationFailed"));
        return RedirectToRoute(NopRouteNames.Standard.MULTIFACTOR_VERIFICATION);
    }

    [HttpPost]
    public async Task<IActionResult> RequestLoginCode()
    {
        var sessionInfo = await HttpContext.Session.GetAsync<CustomerMultiFactorAuthenticationInfo>(NopCustomerDefaults.CustomerMultiFactorAuthenticationInfo);
        if (sessionInfo == null || string.IsNullOrEmpty(sessionInfo.UserName))
            return Json(new { success = false, message = "Login session expired." });

        var customer = _customerSettings.UsernamesEnabled
            ? await _customerService.GetCustomerByUsernameAsync(sessionInfo.UserName)
            : await _customerService.GetCustomerByEmailAsync(sessionInfo.UserName);

        if (customer == null)
            return Json(new { success = false, message = "Customer not found." });

        var phoneNumber = await _genericAttributeService.GetAttributeAsync<string>(customer, SMSDefaults.SMS2FAPhoneNumberAttribute);
        if (string.IsNullOrEmpty(phoneNumber))
            phoneNumber = customer.Phone;

        if (string.IsNullOrEmpty(phoneNumber))
            return Json(new { success = false, message = "No registered phone number found for this account." });

        try
        {
            var plainCodeForTesting = await _smsService.GenerateAndSendCodeAsync(customer.Email ?? customer.Username, phoneNumber);
            return Json(new { success = true, message = "Code sent successfully.", code = plainCodeForTesting });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Failed to send code: {ex.Message}" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> DeviceVerificationChallenge()
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        if (currentCustomer == null)
            return RedirectToRoute(NopRouteNames.General.LOGIN);

        var model = new SMSTokenModel();
        return View("~/Plugins/MultiFactorAuth.SMS/Views/Customer/DeviceVerificationChallenge.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> VerifyDeviceToken(SMSTokenModel model)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        if (currentCustomer == null)
            return RedirectToRoute(NopRouteNames.General.LOGIN);

        var isValid = await _smsService.ValidateCodeAsync(currentCustomer.Email ?? currentCustomer.Username, model.Token);
        if (isValid)
        {
            var deviceToken = Guid.NewGuid().ToString("N");
            var userAgent = Request.Headers["User-Agent"].ToString();
            var deviceName = string.IsNullOrEmpty(userAgent) ? "Browser Device" : userAgent.Split(' ').FirstOrDefault() ?? "Browser Device";

            await _securityRestrictionService.RegisterDeviceAsync(currentCustomer.Id, deviceToken, deviceName, isApproved: true);

            var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.UtcNow.AddYears(1)
            };
            Response.Cookies.Append("SMS2FA.DeviceToken", deviceToken, cookieOptions);

            _notificationService.SuccessNotification("Device registered and approved successfully.");
            
            if (await _customerService.IsAdminAsync(currentCustomer))
                return RedirectToAction("Index", "Home", new { area = AreaNames.ADMIN });
            
            return RedirectToRoute(NopRouteNames.General.HOMEPAGE);
        }

        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.MultiFactorAuth.SMS.Customer.VerificationFailed"));
        return RedirectToAction("DeviceVerificationChallenge");
    }
}
