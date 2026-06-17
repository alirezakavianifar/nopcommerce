namespace Nop.Plugin.Misc.GroupPurchase.Domain;

/// <summary>
/// Represents the type of reward
/// </summary>
public enum RewardType
{
    /// <summary>
    /// Wallet credit
    /// </summary>
    WalletCredit = 10,
    
    /// <summary>
    /// Lottery points
    /// </summary>
    LotteryPoints = 20,
    
    /// <summary>
    /// Discount
    /// </summary>
    Discount = 30
}
