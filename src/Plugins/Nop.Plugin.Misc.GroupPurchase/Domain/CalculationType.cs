namespace Nop.Plugin.Misc.GroupPurchase.Domain;

/// <summary>
/// Represents the type of reward calculation
/// </summary>
public enum CalculationType
{
    /// <summary>
    /// Fixed amount
    /// </summary>
    Fixed = 10,
    
    /// <summary>
    /// Percentage of cart total
    /// </summary>
    PercentageOfCartTotal = 20,
    
    /// <summary>
    /// Percentage of net profit
    /// </summary>
    PercentageOfNetProfit = 30
}
