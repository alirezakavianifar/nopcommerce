using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.UserNotifications.Services;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.UserNotifications.Components;

/// <summary>
/// Represents the user notifications view component
/// </summary>
public class UserNotificationsViewComponent : NopViewComponent
{
    protected readonly IUserNotificationService _notificationService;
    protected readonly IWorkContext _workContext;
    protected readonly ICustomerService _customerService;

    public UserNotificationsViewComponent(
        IUserNotificationService notificationService,
        IWorkContext workContext,
        ICustomerService customerService)
    {
        _notificationService = notificationService;
        _workContext = workContext;
        _customerService = customerService;
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
        if (HttpContext.Items.ContainsKey("NotificationTickerRendered"))
            return Content(string.Empty);

        var customer = await _workContext.GetCurrentCustomerAsync();
        IList<int> roleIds = null;
        if (customer != null)
        {
            roleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
        }

        var announcements = await _notificationService.GetActiveAnnouncementsAsync(roleIds);

        if (!announcements.Any())
            return Content(string.Empty);

        HttpContext.Items["NotificationTickerRendered"] = true;
        return View("~/Plugins/Misc.UserNotifications/Views/Public/Announcements.cshtml", announcements);
    }
}
