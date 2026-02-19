using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.UserNotifications.Services;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.UserNotifications.Components;

/// <summary>
/// Represents the user notifications view component
/// </summary>
public class UserNotificationsViewComponent : NopViewComponent
{
    protected readonly IUserNotificationService _notificationService;

    public UserNotificationsViewComponent(IUserNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Invoke view component
    /// </summary>
    /// <param name="widgetZone">Widget zone name</param>
    /// <param name="additionalData">Additional data</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the view component result
    /// </returns>
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var announcements = await _notificationService.GetActiveAnnouncementsAsync();

        if (!announcements.Any())
            return Content(string.Empty);

        return View("~/Plugins/Misc.UserNotifications/Views/Public/Announcements.cshtml", announcements);
    }
}
