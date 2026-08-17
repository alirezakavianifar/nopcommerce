using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.UserNotifications.Models;

public record CustomerInboxItemModel : BaseNopEntityModel
{
    public string Title { get; set; }
    public string Message { get; set; }
    public string ActionUrl { get; set; }
    public string Category { get; set; } = "System"; // Order, Promotion, System, Product, Security
    public string Icon { get; set; } = "fa-bell";
    public string ImageUrl { get; set; }
    public string CouponCode { get; set; }
    public DateTime? ExpiresOnUtc { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public string RelativeTime { get; set; }
    public bool IsExpired => ExpiresOnUtc.HasValue && ExpiresOnUtc.Value < DateTime.UtcNow;
}

public record CustomerInboxListModel : BaseNopModel
{
    public int UnreadCount { get; set; }
    public string CurrentCategory { get; set; } = "All";
    public string SearchKeyword { get; set; }
    public bool? UnreadOnly { get; set; }
    public IList<CustomerInboxItemModel> Items { get; set; } = new List<CustomerInboxItemModel>();
    public int PageIndex { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

public record HeaderNotificationsFlyoutModel : BaseNopModel
{
    public int UnreadCount { get; set; }
    public IList<CustomerInboxItemModel> RecentItems { get; set; } = new List<CustomerInboxItemModel>();
}

public record CustomerNotificationPreferenceModel : BaseNopModel
{
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
    public bool OnSiteToastsEnabled { get; set; }
    public bool SoundEnabled { get; set; }
    public bool OrderUpdatesEnabled { get; set; }
    public bool PromotionsEnabled { get; set; }
    public bool SystemAnnouncementsEnabled { get; set; }
}
