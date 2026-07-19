using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.MultiFactorAuth.SMS.Components;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.MultiFactorAuth.SMS;

/// <summary>
/// Represents method for the multi-factor authentication with SMS OTP and IP/Device Binding widget
/// </summary>
public class SMSMethod : BasePlugin, IMultiFactorAuthenticationMethod, IWidgetPlugin
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly INopUrlHelper _nopUrlHelper;
    protected readonly ISettingService _settingService;
    protected readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public SMSMethod(ILocalizationService localizationService,
        INopUrlHelper nopUrlHelper,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _localizationService = localizationService;
        _nopUrlHelper = nopUrlHelper;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return _nopUrlHelper.RouteUrl(SMSDefaults.ConfigurationRouteName);
    }

    /// <summary>
    /// Gets a type of a view component for displaying plugin in public store (Enrollment/Settings)
    /// </summary>
    public Type GetPublicViewComponent()
    {
        return typeof(SMSAuthenticationViewComponent);
    }

    /// <summary>
    /// Gets a type of a view component for displaying verification page (Login challenge)
    /// </summary>
    public Type GetVerificationViewComponent()
    {
        return typeof(SMSVerificationViewComponent);
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    public override async Task InstallAsync()
    {
        // Default settings
        await _settingService.SaveSettingAsync(new SMSSettings
        {
            Provider = "Generic",
            CodeLength = 6,
            CodeLifetimeMinutes = 3,
            Force2FAForAdmins = false,
            Force2FAForVendors = false,
            EnableIpAllowlist = false,
            EnableDeviceBinding = false,
            ForceDeviceBindingForAdmins = false,
            ForceDeviceBindingForVendors = false
        });

        // Locales/Resources
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.MultiFactorAuth.SMS.Provider"] = "SMS Provider",
            ["Plugins.MultiFactorAuth.SMS.Provider.Hint"] = "Choose your SMS gateway provider (Twilio, Melipayamak, Kavenegar, Generic).",
            ["Plugins.MultiFactorAuth.SMS.ApiKey"] = "API Key / Username",
            ["Plugins.MultiFactorAuth.SMS.ApiKey.Hint"] = "Provide API key or username for the SMS service.",
            ["Plugins.MultiFactorAuth.SMS.ApiSecret"] = "API Secret / Password",
            ["Plugins.MultiFactorAuth.SMS.ApiSecret.Hint"] = "Provide API secret or password if required by the gateway.",
            ["Plugins.MultiFactorAuth.SMS.SenderNumber"] = "Sender Number / Webhook URL",
            ["Plugins.MultiFactorAuth.SMS.SenderNumber.Hint"] = "Provide the registered sender number or URL endpoint for generic webhook.",
            ["Plugins.MultiFactorAuth.SMS.CodeLength"] = "OTP Code Length",
            ["Plugins.MultiFactorAuth.SMS.CodeLifetimeMinutes"] = "OTP Lifetime (minutes)",
            ["Plugins.MultiFactorAuth.SMS.Force2FAForAdmins"] = "Force SMS 2FA for Administrators",
            ["Plugins.MultiFactorAuth.SMS.Force2FAForVendors"] = "Force SMS 2FA for Vendors",
            ["Plugins.MultiFactorAuth.SMS.EnableIpAllowlist"] = "Enable IP Allowlist Restrictions",
            ["Plugins.MultiFactorAuth.SMS.EnableDeviceBinding"] = "Enable Device / Browser Binding",
            ["Plugins.MultiFactorAuth.SMS.ForceDeviceBindingForAdmins"] = "Force Device Binding for Admins",
            ["Plugins.MultiFactorAuth.SMS.ForceDeviceBindingForVendors"] = "Force Device Binding for Vendors",

            ["Plugins.MultiFactorAuth.SMS.Customer.PhoneNumber"] = "Phone Number",
            ["Plugins.MultiFactorAuth.SMS.Customer.PhoneNumber.Required"] = "Please enter a valid phone number.",
            ["Plugins.MultiFactorAuth.SMS.Customer.VerificationCode"] = "SMS Verification Code",
            ["Plugins.MultiFactorAuth.SMS.Customer.VerificationCode.Required"] = "Please enter the verification code sent to your phone.",
            ["Plugins.MultiFactorAuth.SMS.Customer.SendCode"] = "Send OTP Code",
            ["Plugins.MultiFactorAuth.SMS.Customer.VerifyAndEnable"] = "Verify and Enable SMS 2FA",
            ["Plugins.MultiFactorAuth.SMS.Customer.SMSDisabled"] = "SMS 2FA is currently disabled.",
            ["Plugins.MultiFactorAuth.SMS.Customer.SMSEnabled"] = "SMS 2FA is active for your account.",
            ["Plugins.MultiFactorAuth.SMS.Customer.DisableButton"] = "Disable SMS 2FA",
            ["Plugins.MultiFactorAuth.SMS.Customer.Instruction"] = "Configure SMS-based 2FA to secure your account. Enter your phone number below to receive a verification OTP code.",
            ["Plugins.MultiFactorAuth.SMS.Customer.InstructionVerification"] = "An OTP code has been sent to your registered phone number. Enter the code below to complete verification.",
            ["Plugins.MultiFactorAuth.SMS.Customer.VerificationFailed"] = "Invalid verification code or code has expired.",
            ["Plugins.MultiFactorAuth.SMS.Customer.VerificationSuccess"] = "SMS Two-Factor Authentication configured successfully.",
            ["Plugins.MultiFactorAuth.SMS.Customer.CodeSent"] = "Verification code has been sent to your phone.",
            ["Plugins.MultiFactorAuth.SMS.Description"] = "Enables SMS-based Two-Factor authentication by sending an OTP code via a configured gateway.",
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    public override async Task UninstallAsync()
    {
        await _settingService.DeleteSettingAsync<SMSSettings>();
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.MultiFactorAuth.SMS");
        await base.UninstallAsync();
    }

    /// <summary>
    /// Gets a description that will be displayed on customer info page
    /// </summary>
    public async Task<string> GetDescriptionAsync()
    {
        return await _localizationService.GetResourceAsync("Plugins.MultiFactorAuth.SMS.Description");
    }

    #endregion

    #region IWidgetPlugin Members

    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
    /// </summary>
    public bool HideInWidgetList => true;

    /// <summary>
    /// Gets widget zones where this widget should be rendered
    /// </summary>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> { AdminWidgetZones.CustomerDetailsBlock });
    }

    /// <summary>
    /// Gets a type of a view component for displaying widget
    /// </summary>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (string.Equals(widgetZone, AdminWidgetZones.CustomerDetailsBlock, StringComparison.InvariantCultureIgnoreCase))
        {
            return typeof(SMSCustomerSecurityRestrictionsViewComponent);
        }
        return null;
    }

    #endregion

    #region Properties

    public MultiFactorAuthenticationType Type => MultiFactorAuthenticationType.SMSVerification;

    #endregion
}
