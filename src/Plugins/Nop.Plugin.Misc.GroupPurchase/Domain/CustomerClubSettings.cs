using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.GroupPurchase.Domain;

/// <summary>
/// Settings for Customer Club dual conversion and reward bounds
/// </summary>
public class CustomerClubSettings : ISettings
{
    /// <summary>
    /// Points required per 1 Lottery Chance (e.g. 10 points = 1 chance)
    /// </summary>
    public decimal PointsToLotteryChanceRatio { get; set; } = 10m;

    /// <summary>
    /// Currency amount per 1 Point (e.g. 1 point = 1000 Currency units)
    /// </summary>
    public decimal PointsToCurrencyRatio { get; set; } = 1000m;

    /// <summary>
    /// Whether customer club dual conversion is active
    /// </summary>
    public bool EnableDualConversion { get; set; } = true;

    /// <summary>
    /// Default minimum reward amount cap across all rules
    /// </summary>
    public decimal? DefaultMinRewardAmount { get; set; }

    /// <summary>
    /// Default maximum reward amount cap across all rules
    /// </summary>
    public decimal? DefaultMaxRewardAmount { get; set; }
}
