using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.UserNotifications.Services;
using Nop.Services.Customers;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.UserNotifications.Controllers;

[Route("api/notifications")]
public class UserNotificationsApiController : BasePluginController
{
    protected readonly IUserNotificationService _notificationService;
    protected readonly IUserInboxService _userInboxService;
    protected readonly IWorkContext _workContext;
    protected readonly ICustomerService _customerService;

    public UserNotificationsApiController(
        IUserNotificationService notificationService,
        IUserInboxService userInboxService,
        IWorkContext workContext,
        ICustomerService customerService)
    {
        _notificationService = notificationService;
        _userInboxService = userInboxService;
        _workContext = workContext;
        _customerService = customerService;
    }

    [HttpGet("active")]
    public virtual async Task<IActionResult> GetActiveAnnouncements()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        IList<int> roleIds = null;
        if (customer != null)
        {
            roleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
        }

        var announcements = await _notificationService.GetActiveAnnouncementsAsync(roleIds);

        var model = announcements.Select(a => new
        {
            a.Id,
            a.Title,
            a.Body,
            a.StartDateUtc,
            a.EndDateUtc,
            a.CreatedOnUtc,
            a.CustomerRoleId
        });

        return Ok(model);
    }

    [HttpPost("mark-read")]
    public virtual async Task<IActionResult> MarkRead([FromQuery] int? id, [FromBody] MarkReadApiRequest model = null)
    {
        var notificationId = id ?? model?.Id ?? 0;
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || await _customerService.IsGuestAsync(customer))
        {
            return Unauthorized(new { success = false, message = "Customer is not authenticated." });
        }

        if (notificationId > 0)
        {
            await _userInboxService.MarkAsReadAsync(notificationId, customer.Id);
        }
        else
        {
            await _userInboxService.MarkAllAsReadAsync(customer.Id);
        }

        var unreadCount = await _userInboxService.GetUnreadCountAsync(customer.Id);
        return Ok(new { success = true, unreadCount, markedNotificationId = notificationId });
    }
}

public class MarkReadApiRequest
{
    public int Id { get; set; }
}

