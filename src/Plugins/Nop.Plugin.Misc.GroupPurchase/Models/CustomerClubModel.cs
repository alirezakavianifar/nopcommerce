using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.GroupPurchase.Models;

public record CustomerClubModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.CustomerClub.Fields.PointsToLotteryChanceRatio")]
    public decimal PointsToLotteryChanceRatio { get; set; }

    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.CustomerClub.Fields.PointsToCurrencyRatio")]
    public decimal PointsToCurrencyRatio { get; set; }

    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.CustomerClub.Fields.EnableDualConversion")]
    public bool EnableDualConversion { get; set; }

    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.CustomerClub.Fields.DefaultMinRewardAmount")]
    public decimal? DefaultMinRewardAmount { get; set; }

    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.CustomerClub.Fields.DefaultMaxRewardAmount")]
    public decimal? DefaultMaxRewardAmount { get; set; }
}
