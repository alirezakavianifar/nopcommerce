using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.UserNotifications.Models;
using Nop.Plugin.Misc.UserNotifications.Services;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.UserNotifications.Components;

public class NotificationsInboxViewComponent : NopViewComponent
{
    private readonly IUserInboxService _userInboxService;
    private readonly ICustomerService _customerService;
    private readonly IWorkContext _workContext;

    public NotificationsInboxViewComponent(
        IUserInboxService userInboxService,
        ICustomerService customerService,
        IWorkContext workContext)
    {
        _userInboxService = userInboxService;
        _customerService = customerService;
        _workContext = workContext;
    }

    private static string GetRelativeTime(DateTime createdOnUtc)
    {
        var ts = DateTime.UtcNow - createdOnUtc;
        if (ts.TotalMinutes < 1)
            return "Just now";
        if (ts.TotalMinutes < 60)
            return $"{(int)ts.TotalMinutes}m ago";
        if (ts.TotalHours < 24)
            return $"{(int)ts.TotalHours}h ago";
        if (ts.TotalDays < 2)
            return "Yesterday";
        if (ts.TotalDays < 30)
            return $"{(int)ts.TotalDays}d ago";
        return createdOnUtc.ToString("MMM dd, yyyy");
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || await _customerService.IsGuestAsync(customer))
            return Content(string.Empty);

        var unreadCount = await _userInboxService.GetUnreadCountAsync(customer.Id);
        var recentMessages = await _userInboxService.GetRecentCustomerInboxAsync(customer.Id, 8);

        var model = new HeaderNotificationsFlyoutModel
        {
            UnreadCount = unreadCount,
            RecentItems = recentMessages.Select(m => new CustomerInboxItemModel
            {
                Id = m.Id,
                Title = m.Title,
                Message = m.Message,
                ActionUrl = m.ActionUrl,
                Category = m.Category ?? "System",
                Icon = m.Icon ?? "fa-bell",
                ImageUrl = m.ImageUrl,
                CouponCode = m.CouponCode,
                ExpiresOnUtc = m.ExpiresOnUtc,
                IsRead = m.IsRead,
                CreatedOnUtc = m.CreatedOnUtc,
                RelativeTime = GetRelativeTime(m.CreatedOnUtc)
            }).ToList()
        };

        return View("~/Plugins/Misc.UserNotifications/Views/Shared/Components/NotificationsInbox/Default.cshtml", model);
    }
}
