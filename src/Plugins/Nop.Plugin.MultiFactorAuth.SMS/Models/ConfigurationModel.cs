using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.MultiFactorAuth.SMS.Models;

public record ConfigurationModel : BaseNopModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.MultiFactorAuth.SMS.Provider")]
    public string Provider { get; set; }

    [NopResourceDisplayName("Plugins.MultiFactorAuth.SMS.ApiKey")]
    public string ApiKey { get; set; }

    [NopResourceDisplayName("Plugins.MultiFactorAuth.SMS.ApiSecret")]
    public string ApiSecret { get; set; }

    [NopResourceDisplayName("Plugins.MultiFactorAuth.SMS.SenderNumber")]
    public string SenderNumber { get; set; }

    [NopResourceDisplayName("Plugins.MultiFactorAuth.SMS.CodeLength")]
    public int CodeLength { get; set; }

    [NopResourceDisplayName("Plugins.MultiFactorAuth.SMS.CodeLifetimeMinutes")]
    public int CodeLifetimeMinutes { get; set; }

    [NopResourceDisplayName("Plugins.MultiFactorAuth.SMS.Force2FAForAdmins")]
    public bool Force2FAForAdmins { get; set; }

    [NopResourceDisplayName("Plugins.MultiFactorAuth.SMS.Force2FAForVendors")]
    public bool Force2FAForVendors { get; set; }

    [NopResourceDisplayName("Plugins.MultiFactorAuth.SMS.EnableIpAllowlist")]
    public bool EnableIpAllowlist { get; set; }

    [NopResourceDisplayName("Plugins.MultiFactorAuth.SMS.EnableDeviceBinding")]
    public bool EnableDeviceBinding { get; set; }

    [NopResourceDisplayName("Plugins.MultiFactorAuth.SMS.ForceDeviceBindingForAdmins")]
    public bool ForceDeviceBindingForAdmins { get; set; }

    [NopResourceDisplayName("Plugins.MultiFactorAuth.SMS.ForceDeviceBindingForVendors")]
    public bool ForceDeviceBindingForVendors { get; set; }
}
