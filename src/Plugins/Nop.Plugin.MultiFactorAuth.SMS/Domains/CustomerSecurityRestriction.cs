using System;
using Nop.Core;

namespace Nop.Plugin.MultiFactorAuth.SMS.Domains;

/// <summary>
/// Represents per-user IP allowlists and bound devices/browsers
/// </summary>
public class CustomerSecurityRestriction : BaseEntity
{
    /// <summary>
    /// Gets or sets the customer ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the comma-separated allowed IP addresses or CIDR blocks
    /// </summary>
    public string AllowedIpAddresses { get; set; }

    /// <summary>
    /// Gets or sets the cryptographically hashed device/browser token (cookie token)
    /// </summary>
    public string DeviceTokenHash { get; set; }

    /// <summary>
    /// Gets or sets the friendly name/description of the device/browser
    /// </summary>
    public string DeviceName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this device binding is approved/trusted
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the device was registered in UTC
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the device was last used to log in in UTC
    /// </summary>
    public DateTime? LastUsedUtc { get; set; }
}
