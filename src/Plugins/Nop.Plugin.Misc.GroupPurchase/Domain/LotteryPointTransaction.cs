using Nop.Core;

namespace Nop.Plugin.Misc.GroupPurchase.Domain;

/// <summary>
/// Represents a lottery point transaction
/// </summary>
public partial class LotteryPointTransaction : BaseEntity
{
    /// <summary>
    /// Gets or sets the customer identifier
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the points amount (can be negative)
    /// </summary>
    public int Points { get; set; }

    /// <summary>
    /// Gets or sets the source identifier
    /// </summary>
    public int SourceId { get; set; }

    /// <summary>
    /// Gets or sets the related group purchase identifier (optional)
    /// </summary>
    public int? GroupPurchaseId { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the transaction
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the source
    /// </summary>
    public LotterySource Source
    {
        get => (LotterySource)SourceId;
        set => SourceId = (int)value;
    }
}
