using Nop.Core;

namespace Nop.Plugin.Misc.GroupPurchase.Domain;

/// <summary>
/// Represents a group purchase reward
/// </summary>
public partial class GroupPurchaseReward : BaseEntity
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
    /// Gets or sets the reward type identifier
    /// </summary>
    public int RewardTypeId { get; set; }

    /// <summary>
    /// Gets or sets the reward amount (value of the actual rewarded amount)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the calculation type identifier
    /// </summary>
    public int CalculationTypeId { get; set; }

    /// <summary>
    /// Gets or sets the category identifier (if applicable)
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the date and time of record creation
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the reward type
    /// </summary>
    public RewardType RewardType
    {
        get => (RewardType)RewardTypeId;
        set => RewardTypeId = (int)value;
    }

    /// <summary>
    /// Gets or sets the calculation type
    /// </summary>
    public CalculationType CalculationType
    {
        get => (CalculationType)CalculationTypeId;
        set => CalculationTypeId = (int)value;
    }
}
