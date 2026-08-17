using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.UserNotifications.Components;

public class AccountNavigationNotificationsViewComponent : NopViewComponent
{
    public Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        return Task.FromResult<IViewComponentResult>(
            View("~/Plugins/Misc.UserNotifications/Views/Shared/Components/AccountNavigationLinks/Default.cshtml")
        );
    }
}
