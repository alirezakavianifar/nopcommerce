using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Misc.SellerMarketing.Components;

public class SellerDashboardNavigationViewComponent : NopViewComponent
{
    #region Fields

    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public SellerDashboardNavigationViewComponent(IWorkContext workContext)
    {
        _workContext = workContext;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (widgetZone != PublicWidgetZones.AccountNavigationAfter)
            return Content("");

        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || customer.VendorId == 0)
            return Content("");

        return View("~/Plugins/Misc.SellerMarketing/Views/Public/_SellerDashboardNavigation.cshtml");
    }

    #endregion
}
