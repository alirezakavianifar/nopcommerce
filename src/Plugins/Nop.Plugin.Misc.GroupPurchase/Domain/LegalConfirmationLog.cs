using Nop.Core;

namespace Nop.Plugin.Misc.GroupPurchase.Domain;

/// <summary>
/// Represents a legal confirmation log
/// </summary>
public partial class LegalConfirmationLog : BaseEntity
{
    /// <summary>
    /// Gets or sets the customer identifier
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the group purchase identifier
    /// </summary>
    public int GroupPurchaseId { get; set; }

    /// <summary>
    /// Gets or sets the confirmation type (e.g. LeaderAgreement, MemberAgreement)
    /// </summary>
    public string ConfirmationType { get; set; }

    /// <summary>
    /// Gets or sets the IP address
    /// </summary>
    public string IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the creation date
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }
}
