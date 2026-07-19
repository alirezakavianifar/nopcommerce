using Nop.Core.Configuration;

namespace Nop.Plugin.MultiFactorAuth.SMS;

/// <summary>
/// Represents settings of the SMS Multi-factor authentication method
/// </summary>
public class SMSSettings : ISettings
{
    /// <summary>
    /// Gets or sets the selected SMS Provider (e.g. "Twilio", "Melipayamak", "Kavenegar", "Generic")
    /// </summary>
    public string Provider { get; set; } = "Generic";

    /// <summary>
    /// Gets or sets the API Key or Token
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the API Secret / Password (used for some providers)
    /// </summary>
    public string ApiSecret { get; set; }

    /// <summary>
    /// Gets or sets the Sender Number or Sender ID
    /// </summary>
    public string SenderNumber { get; set; }

    /// <summary>
    /// Gets or sets the length of the verification code
    /// </summary>
    public int CodeLength { get; set; } = 6;

    /// <summary>
    /// Gets or sets the lifetime of the verification code in minutes
    /// </summary>
    public int CodeLifetimeMinutes { get; set; } = 3;

    /// <summary>
    /// Gets or sets a value indicating whether SMS 2FA is forced for Administrators role
    /// </summary>
    public bool Force2FAForAdmins { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether SMS 2FA is forced for Vendors role
    /// </summary>
    public bool Force2FAForVendors { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether IP allowlisting is enabled globally
    /// </summary>
    public bool EnableIpAllowlist { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether Device/Browser binding is enabled globally
    /// </summary>
    public bool EnableDeviceBinding { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether Device Binding is forced for Administrators
    /// </summary>
    public bool ForceDeviceBindingForAdmins { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether Device Binding is forced for Vendors
    /// </summary>
    public bool ForceDeviceBindingForVendors { get; set; } = false;
}
