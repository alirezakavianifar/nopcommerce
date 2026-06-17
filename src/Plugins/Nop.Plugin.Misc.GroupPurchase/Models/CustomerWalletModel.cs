namespace Nop.Plugin.Misc.GroupPurchase.Models;

public record CustomerWalletModel
{
    public decimal RegularBalance { get; set; }
    public decimal GroupRewardBalance { get; set; }
}
