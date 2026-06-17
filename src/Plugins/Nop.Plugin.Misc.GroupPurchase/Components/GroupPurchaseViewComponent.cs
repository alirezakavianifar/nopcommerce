using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.GroupPurchase.Services;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.GroupPurchase.Components;

public class GroupPurchaseViewComponent : NopViewComponent
{
    protected readonly IGroupPurchaseService _groupPurchaseService;
    protected readonly IWorkContext _workContext;

    public GroupPurchaseViewComponent(IGroupPurchaseService groupPurchaseService,
        IWorkContext workContext)
    {
        _groupPurchaseService = groupPurchaseService;
        _workContext = workContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        // We only want to show this in the shopping cart for now
        if (widgetZone != PublicWidgetZones.OrderSummaryCartFooter)
            return Content("");

        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null)
            return Content("");

        // Check if already in a group purchase
        // var groupPurchaseId = await _genericAttributeService.GetAttributeAsync<int>(customer, "GroupPurchaseId");

        return View("~/Plugins/Misc.GroupPurchase/Views/Public/GroupPurchaseWidget.cshtml");
    }
}
