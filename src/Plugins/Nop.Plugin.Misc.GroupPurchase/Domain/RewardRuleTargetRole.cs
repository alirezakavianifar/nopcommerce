namespace Nop.Plugin.Misc.GroupPurchase.Domain;

/// <summary>
/// Represents the role targeted by the reward rule
/// </summary>
public enum RewardRuleTargetRole
{
    /// <summary>
    /// The leader of the group purchase
    /// </summary>
    Leader = 10,
    
    /// <summary>
    /// A subgroup member of the group purchase
    /// </summary>
    Subgroup = 20
}
