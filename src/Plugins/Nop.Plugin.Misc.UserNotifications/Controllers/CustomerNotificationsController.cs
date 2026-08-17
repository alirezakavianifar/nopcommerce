using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.UserNotifications.Domain;
using Nop.Plugin.Misc.UserNotifications.Models;
using Nop.Plugin.Misc.UserNotifications.Services;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.UserNotifications.Controllers;

public class CustomerNotificationsController : BasePluginController
{
    private readonly IUserInboxService _userInboxService;
    private readonly IPopupNotificationService _popupNotificationService;
    private readonly INotificationPreferenceService _preferenceService;
    private readonly ICustomerService _customerService;
    private readonly IWorkContext _workContext;
    private readonly ILocalizationService _localizationService;

    public CustomerNotificationsController(
        IUserInboxService userInboxService,
        IPopupNotificationService popupNotificationService,
        INotificationPreferenceService preferenceService,
        ICustomerService customerService,
        IWorkContext workContext,
        ILocalizationService localizationService)
    {
        _userInboxService = userInboxService;
        _popupNotificationService = popupNotificationService;
        _preferenceService = preferenceService;
        _customerService = customerService;
        _workContext = workContext;
        _localizationService = localizationService;
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

    [HttpGet("customer/notifications")]
    public virtual async Task<IActionResult> Inbox(string category = "All", string q = null, bool? unread = null, int page = 1)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || await _customerService.IsGuestAsync(customer))
            return Challenge();

        var pageIndex = Math.Max(0, page - 1);
        const int pageSize = 15;

        var messages = await _userInboxService.GetCustomerInboxAsync(customer.Id, category, unread, q, pageIndex, pageSize);
        var unreadCount = await _userInboxService.GetUnreadCountAsync(customer.Id);

        var model = new CustomerInboxListModel
        {
            UnreadCount = unreadCount,
            CurrentCategory = category ?? "All",
            SearchKeyword = q,
            UnreadOnly = unread,
            PageIndex = pageIndex,
            TotalPages = messages.TotalPages,
            HasNextPage = messages.HasNextPage,
            HasPreviousPage = messages.HasPreviousPage,
            Items = messages.Select(m => new CustomerInboxItemModel
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

        return View("~/Plugins/Misc.UserNotifications/Views/Public/Inbox.cshtml", model);
    }

    [HttpGet("customer/notifications/flyout-items")]
    public virtual async Task<IActionResult> GetFlyoutItems(string category = "All")
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || await _customerService.IsGuestAsync(customer))
            return Json(new { success = false, items = new List<object>(), unreadCount = 0 });

        var messages = await _userInboxService.GetCustomerInboxAsync(customer.Id, category, null, null, 0, 10);
        var unreadCount = await _userInboxService.GetUnreadCountAsync(customer.Id);

        var items = messages.Select(m => new
        {
            m.Id,
            m.Title,
            m.Message,
            m.ActionUrl,
            Category = m.Category ?? "System",
            Icon = m.Icon ?? "fa-bell",
            m.ImageUrl,
            m.CouponCode,
            m.IsRead,
            RelativeTime = GetRelativeTime(m.CreatedOnUtc),
            IsExpired = m.ExpiresOnUtc.HasValue && m.ExpiresOnUtc.Value < DateTime.UtcNow
        });

        return Json(new { success = true, items, unreadCount });
    }

    [HttpGet("customer/notifications/poll")]
    public virtual async Task<IActionResult> PollNotifications()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || await _customerService.IsGuestAsync(customer))
            return Json(new { success = false, unreadCount = 0, popups = new List<object>() });

        var unreadCount = await _userInboxService.GetUnreadCountAsync(customer.Id);
        var popups = await _popupNotificationService.GetActivePopupsForCustomerAsync(customer.Id);

        var popupData = popups.Select(p => new
        {
            p.Id,
            p.Title,
            p.Message,
            p.ActionUrl,
            PopupType = p.PopupType ?? "Toast",
            Category = p.Category ?? "Promotion",
            Icon = p.Icon ?? "fa-bell",
            p.ImageUrl,
            p.CouponCode,
            ExpiresInSeconds = p.ExpiresOnUtc.HasValue ? (int?)(p.ExpiresOnUtc.Value - DateTime.UtcNow).TotalSeconds : null
        });

        return Json(new { success = true, unreadCount, popups = popupData });
    }

    [HttpPost("customer/notifications/mark-read")]
    public virtual async Task<IActionResult> MarkAsRead(int id)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer != null && !await _customerService.IsGuestAsync(customer))
        {
            await _userInboxService.MarkAsReadAsync(id, customer.Id);
            var unreadCount = await _userInboxService.GetUnreadCountAsync(customer.Id);
            return Ok(new { success = true, unreadCount });
        }
        return Ok(new { success = true, unreadCount = 0 });
    }

    [HttpPost("customer/notifications/mark-all-read")]
    public virtual async Task<IActionResult> MarkAllAsRead()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer != null && !await _customerService.IsGuestAsync(customer))
        {
            await _userInboxService.MarkAllAsReadAsync(customer.Id);
            return Ok(new { success = true, unreadCount = 0 });
        }
        return Ok(new { success = true, unreadCount = 0 });
    }

    [HttpPost("customer/notifications/delete")]
    public virtual async Task<IActionResult> DeleteNotification(int id)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer != null && !await _customerService.IsGuestAsync(customer))
        {
            await _userInboxService.DeleteMessageAsync(id, customer.Id);
            var unreadCount = await _userInboxService.GetUnreadCountAsync(customer.Id);
            return Ok(new { success = true, unreadCount });
        }
        return Ok(new { success = true });
    }

    [HttpPost("customer/notifications/clear-read")]
    public virtual async Task<IActionResult> ClearRead()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer != null && !await _customerService.IsGuestAsync(customer))
        {
            await _userInboxService.ClearReadMessagesAsync(customer.Id);
            return Ok(new { success = true });
        }
        return Ok(new { success = true });
    }

    [HttpPost("customer/notifications/dismiss-popup")]
    public virtual async Task<IActionResult> DismissPopup(int id)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer != null && !await _customerService.IsGuestAsync(customer))
        {
            await _popupNotificationService.DismissPopupAsync(id, customer.Id);
        }
        return Ok(new { success = true });
    }

    [HttpPost("customer/notifications/dismiss-all-popups")]
    public virtual async Task<IActionResult> DismissAllPopups()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer != null && !await _customerService.IsGuestAsync(customer))
        {
            await _popupNotificationService.DismissAllPopupsAsync(customer.Id);
        }
        return Ok(new { success = true });
    }

    [HttpGet("customer/notifications/preferences")]
    public virtual async Task<IActionResult> Preferences()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || await _customerService.IsGuestAsync(customer))
            return Challenge();

        var prefs = await _preferenceService.GetCustomerPreferencesAsync(customer.Id);

        var model = new CustomerNotificationPreferenceModel
        {
            EmailEnabled = prefs.EmailEnabled,
            SmsEnabled = prefs.SmsEnabled,
            OnSiteToastsEnabled = prefs.OnSiteToastsEnabled,
            SoundEnabled = prefs.SoundEnabled,
            OrderUpdatesEnabled = prefs.OrderUpdatesEnabled,
            PromotionsEnabled = prefs.PromotionsEnabled,
            SystemAnnouncementsEnabled = prefs.SystemAnnouncementsEnabled
        };

        return View("~/Plugins/Misc.UserNotifications/Views/Public/Preferences.cshtml", model);
    }

    [HttpPost("customer/notifications/preferences")]
    public virtual async Task<IActionResult> Preferences(CustomerNotificationPreferenceModel model)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || await _customerService.IsGuestAsync(customer))
            return Challenge();

        var prefs = await _preferenceService.GetCustomerPreferencesAsync(customer.Id);
        prefs.EmailEnabled = model.EmailEnabled;
        prefs.SmsEnabled = model.SmsEnabled;
        prefs.OnSiteToastsEnabled = model.OnSiteToastsEnabled;
        prefs.SoundEnabled = model.SoundEnabled;
        prefs.OrderUpdatesEnabled = model.OrderUpdatesEnabled;
        prefs.PromotionsEnabled = model.PromotionsEnabled;
        prefs.SystemAnnouncementsEnabled = model.SystemAnnouncementsEnabled;

        await _preferenceService.SaveCustomerPreferencesAsync(prefs);

        ViewBag.SuccessMessage = await _localizationService.GetResourceAsync("Plugins.Misc.UserNotifications.PreferencesSaved") ?? "Your notification preferences have been saved successfully.";

        return View("~/Plugins/Misc.UserNotifications/Views/Public/Preferences.cshtml", model);
    }
}
