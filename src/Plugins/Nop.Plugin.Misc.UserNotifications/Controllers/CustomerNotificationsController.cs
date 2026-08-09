using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.UserNotifications.Models;
using Nop.Plugin.Misc.UserNotifications.Services;
using Nop.Services.Customers;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.UserNotifications.Controllers;

public class CustomerNotificationsController : BasePluginController
{
    private readonly IUserInboxService _userInboxService;
    private readonly IPopupNotificationService _popupNotificationService;
    private readonly ICustomerService _customerService;
    private readonly IWorkContext _workContext;

    public CustomerNotificationsController(
        IUserInboxService userInboxService,
        IPopupNotificationService popupNotificationService,
        ICustomerService customerService,
        IWorkContext workContext)
    {
        _userInboxService = userInboxService;
        _popupNotificationService = popupNotificationService;
        _customerService = customerService;
        _workContext = workContext;
    }

    [HttpGet("customer/notifications")]
    public virtual async Task<IActionResult> Inbox()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || await _customerService.IsGuestAsync(customer))
            return Challenge();

        var messages = await _userInboxService.GetCustomerInboxAsync(customer.Id);
        var unreadCount = await _userInboxService.GetUnreadCountAsync(customer.Id);

        var model = new CustomerInboxListModel
        {
            UnreadCount = unreadCount,
            Items = messages.Select(m => new CustomerInboxItemModel
            {
                Id = m.Id,
                Title = m.Title,
                Message = m.Message,
                ActionUrl = m.ActionUrl,
                IsRead = m.IsRead,
                CreatedOnUtc = m.CreatedOnUtc
            }).ToList()
        };

        return View("~/Plugins/Misc.UserNotifications/Views/Public/Inbox.cshtml", model);
    }

    [HttpPost("customer/notifications/mark-read")]
    public virtual async Task<IActionResult> MarkAsRead(int id)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer != null && !await _customerService.IsGuestAsync(customer))
        {
            await _userInboxService.MarkAsReadAsync(id, customer.Id);
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
}
