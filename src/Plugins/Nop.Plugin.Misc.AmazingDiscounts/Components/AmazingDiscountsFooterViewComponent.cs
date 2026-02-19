using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.AmazingDiscounts.Components;

public class AmazingDiscountsFooterViewComponent : NopViewComponent
{
    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
        return View("~/Plugins/Misc.AmazingDiscounts/Views/Public/FooterLink.cshtml");
    }
}
