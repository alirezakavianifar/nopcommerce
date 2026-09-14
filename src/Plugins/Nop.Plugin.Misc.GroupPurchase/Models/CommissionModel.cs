using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.GroupPurchase.Models;

public record CommissionModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.Commission.Fields.Scope")]
    public int CommissionScopeId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.Commission.Fields.Scope")]
    public string ScopeName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.Commission.Fields.EntityId")]
    public int EntityId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.Commission.Fields.EntityName")]
    public string EntityName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.GroupPurchase.Commission.Fields.Percentage")]
    public decimal Percentage { get; set; }
}

public record CommissionSearchModel : BaseSearchModel
{
    public int? ScopeId { get; set; }
}

public record CommissionListModel : BasePagedListModel<CommissionModel>
{
}
