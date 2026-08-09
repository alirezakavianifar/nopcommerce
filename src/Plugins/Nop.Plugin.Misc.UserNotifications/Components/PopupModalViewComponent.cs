using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.UserNotifications.Services;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.UserNotifications.Components;

public class PopupModalViewComponent : NopViewComponent
{
    private readonly IPopupNotificationService _popupNotificationService;
    private readonly ICustomerService _customerService;
    private readonly IWorkContext _workContext;

    public PopupModalViewComponent(
        IPopupNotificationService popupNotificationService,
        ICustomerService customerService,
        IWorkContext workContext)
    {
        _popupNotificationService = popupNotificationService;
        _customerService = customerService;
        _workContext = workContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || await _customerService.IsGuestAsync(customer))
            return Content(string.Empty);

        var popups = await _popupNotificationService.GetActivePopupsForCustomerAsync(customer.Id);
        if (!popups.Any())
            return Content(string.Empty);

        return View("~/Plugins/Misc.UserNotifications/Views/Shared/Components/PopupModal/Default.cshtml", popups);
    }
}
