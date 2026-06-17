using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Misc.GroupPurchase.Components;

public class CustomerDashboardNavigationViewComponent : NopViewComponent
{
    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
        if (widgetZone != PublicWidgetZones.AccountNavigationAfter)
            return Content("");

        return View("~/Plugins/Misc.GroupPurchase/Views/Public/_CustomerDashboardNavigation.cshtml");
    }
}
