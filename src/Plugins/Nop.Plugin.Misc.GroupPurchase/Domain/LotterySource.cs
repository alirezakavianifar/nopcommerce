namespace Nop.Plugin.Misc.GroupPurchase.Domain;

/// <summary>
/// Represents the source of lottery points
/// </summary>
public enum LotterySource
{
    /// <summary>
    /// Points from being a group leader
    /// </summary>
    GroupPurchaseLeader = 10,
    
    /// <summary>
    /// Points from completing an order
    /// </summary>
    GroupPurchaseMember = 20
}
