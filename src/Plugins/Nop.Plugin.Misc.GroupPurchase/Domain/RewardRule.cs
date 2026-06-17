using Nop.Core;

namespace Nop.Plugin.Misc.GroupPurchase.Domain;

/// <summary>
/// Represents a reward rule
/// </summary>
public partial class RewardRule : BaseEntity
{
    /// <summary>
    /// Gets or sets the target role identifier
    /// </summary>
    public int TargetRoleId { get; set; }

    /// <summary>
    /// Gets or sets the reward type identifier
    /// </summary>
    public int RewardTypeId { get; set; }

    /// <summary>
    /// Gets or sets the calculation type identifier
    /// </summary>
    public int CalculationTypeId { get; set; }

    /// <summary>
    /// Gets or sets the reward value (fixed amount or percentage)
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Gets or sets the category identifier (0 if applies to all)
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the minimum cart amount required for the rule to apply
    /// </summary>
    public decimal MinCartAmount { get; set; }

    /// <summary>
    /// Gets or sets the minimum number of members required for the rule to apply
    /// </summary>
    public int MinMembers { get; set; }

    /// <summary>
    /// Gets or sets the target role
    /// </summary>
    public RewardRuleTargetRole TargetRole
    {
        get => (RewardRuleTargetRole)TargetRoleId;
        set => TargetRoleId = (int)value;
    }

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
