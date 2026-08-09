using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.UserNotifications.Models;

public record CustomerInboxItemModel : BaseNopEntityModel
{
    public string Title { get; set; }
    public string Message { get; set; }
    public string ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}

public record CustomerInboxListModel : BaseNopModel
{
    public int UnreadCount { get; set; }
    public IList<CustomerInboxItemModel> Items { get; set; } = new List<CustomerInboxItemModel>();
}
