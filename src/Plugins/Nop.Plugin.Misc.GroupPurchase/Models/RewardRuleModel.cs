using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.GroupPurchase.Models;

/// <summary>
/// Represents a reward rule model
/// </summary>
public record RewardRuleModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.RewardRule.Fields.TargetRole")]
    public int TargetRoleId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.RewardRule.Fields.TargetRole")]
    public string TargetRoleName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.RewardRule.Fields.RewardType")]
    public int RewardTypeId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.RewardRule.Fields.RewardType")]
    public string RewardTypeName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.RewardRule.Fields.CalculationType")]
    public int CalculationTypeId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.RewardRule.Fields.CalculationType")]
    public string CalculationTypeName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.RewardRule.Fields.Value")]
    public decimal Value { get; set; }

    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.RewardRule.Fields.CategoryId")]
    public int CategoryId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.RewardRule.Fields.MinCartAmount")]
    public decimal MinCartAmount { get; set; }

    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.RewardRule.Fields.MinMembers")]
    public int MinMembers { get; set; }
}
