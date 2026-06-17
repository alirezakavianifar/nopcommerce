using Nop.Core;

namespace Nop.Plugin.Misc.GroupPurchase.Domain;

/// <summary>
/// Represents a group purchase
/// </summary>
public partial class GroupPurchase : BaseEntity
{
    /// <summary>
    /// Gets or sets the leader customer identifier
    /// </summary>
    public int LeaderCustomerId { get; set; }

    /// <summary>
    /// Gets or sets the unique code
    /// </summary>
    public string UniqueCode { get; set; }

    /// <summary>
    /// Gets or sets the status identifier
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the date and time of instance creation in UTC
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the delivery city
    /// </summary>
    public string DeliveryCity { get; set; }

    /// <summary>
    /// Gets or sets the delivery address
    /// </summary>
    public string DeliveryAddress { get; set; }

    /// <summary>
    /// Gets or sets the status
    /// </summary>
    public GroupPurchaseStatus Status
    {
        get => (GroupPurchaseStatus)StatusId;
        set => StatusId = (int)value;
    }
}
