using Microsoft.AspNetCore.Mvc;
using Nop.Core;
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

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || await _customerService.IsGuestAsync(customer))
            return Content(string.Empty);

        var unreadCount = await _userInboxService.GetUnreadCountAsync(customer.Id);
        return View("~/Plugins/Misc.UserNotifications/Views/Shared/Components/NotificationsInbox/Default.cshtml", unreadCount);
    }
}
