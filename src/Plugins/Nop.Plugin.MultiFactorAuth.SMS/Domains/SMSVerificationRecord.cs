using System;
using Nop.Core;

namespace Nop.Plugin.MultiFactorAuth.SMS.Domains;

/// <summary>
/// Represents a temporary record for storing generated OTP codes
/// </summary>
public class SMSVerificationRecord : BaseEntity
{
    /// <summary>
    /// Gets or sets the user email or username
    /// </summary>
    public string UserIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the phone number the code was sent to
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the hashed/plain verification code
    /// </summary>
    public string VerificationCode { get; set; }

    /// <summary>
    /// Gets or sets the expiration date and time in UTC
    /// </summary>
    public DateTime ExpiryUtc { get; set; }

    /// <summary>
    /// Gets or sets the count of attempts to verify this code
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this code has already been verified/used
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the record was created in UTC
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }
}
