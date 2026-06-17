using Nop.Core;

namespace Nop.Plugin.Misc.GroupPurchase.Domain;

/// <summary>
/// Represents a group purchase member
/// </summary>
public partial class GroupPurchaseMember : BaseEntity
{
    /// <summary>
    /// Gets or sets the group purchase identifier
    /// </summary>
    public int GroupPurchaseId { get; set; }

    /// <summary>
    /// Gets or sets the customer identifier
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the member is a leader
    /// </summary>
    public bool IsLeader { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the member accepted the terms
    /// </summary>
    public bool AcceptedTerms { get; set; }

    /// <summary>
    /// Gets or sets the date and time of acceptance in UTC
    /// </summary>
    public DateTime? AcceptedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the visibility type identifier
    /// </summary>
    public int VisibilityTypeId { get; set; }

    /// <summary>
    /// Gets or sets the visibility type
    /// </summary>
    public VisibilityType VisibilityType
    {
        get => (VisibilityType)VisibilityTypeId;
        set => VisibilityTypeId = (int)value;
    }
}
