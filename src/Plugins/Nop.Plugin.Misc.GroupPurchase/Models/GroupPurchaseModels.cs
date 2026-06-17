using Nop.Web.Framework.Models;
using Nop.Web.Framework.Models.DataTables;

namespace Nop.Plugin.Misc.GroupPurchase.Models;

public record GroupPurchaseSearchModel : BaseSearchModel
{
}

public record GroupPurchaseModel : BaseNopEntityModel
{
    public string UniqueCode { get; set; }
    public int LeaderCustomerId { get; set; }
    public string Status { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public string DeliveryCity { get; set; }
}

public record GroupPurchaseListModel : BasePagedListModel<GroupPurchaseModel>
{
}
