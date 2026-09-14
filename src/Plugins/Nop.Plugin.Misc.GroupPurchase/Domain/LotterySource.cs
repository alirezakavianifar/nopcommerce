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
    GroupPurchaseMember = 20,

    /// <summary>
    /// Conversion to lottery tickets or wallet credit in customer club
    /// </summary>
    CustomerClubConversion = 30,

    /// <summary>
    /// Manual adjustment by admin
    /// </summary>
    ManualAdjustment = 40
}
