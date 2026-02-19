using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.UserNotifications.Services;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.UserNotifications.Controllers;

[Route("api/notifications")]
public class UserNotificationsApiController : BasePluginController
{
    protected readonly IUserNotificationService _notificationService;

    public UserNotificationsApiController(IUserNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("active")]
    public virtual async Task<IActionResult> GetActiveAnnouncements()
    {
        var announcements = await _notificationService.GetActiveAnnouncementsAsync();

        var model = announcements.Select(a => new
        {
            a.Id,
            a.Title,
            a.Body,
            a.StartDateUtc,
            a.EndDateUtc,
            a.CreatedOnUtc
        });

        return Ok(model);
    }
}
