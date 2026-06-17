using Nop.Plugin.Misc.GroupPurchase.Domain;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.GroupPurchase.Models;

public record CustomerLeaderGroupListModel : BasePagedListModel<CustomerLeaderGroupModel>
{
}

public record CustomerLeaderGroupModel : BaseNopEntityModel
{
    public string UniqueCode { get; set; }
    public string Status { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public int MembersCount { get; set; }
    public string DeliveryCity { get; set; }
}

public record CustomerSubgroupHistoryListModel : BasePagedListModel<CustomerSubgroupModel>
{
}

public record CustomerSubgroupModel : BaseNopEntityModel
{
    public string UniqueCode { get; set; }
    public string Status { get; set; }
    public DateTime JoinedOnUtc { get; set; }
    public string LeaderEmail { get; set; }
    public VisibilityType VisibilityType { get; set; }
}
