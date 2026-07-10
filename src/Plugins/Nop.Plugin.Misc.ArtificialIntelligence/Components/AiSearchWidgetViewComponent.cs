using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Components;

public class AiSearchWidgetViewComponent : NopViewComponent
{
    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
        if (widgetZone != PublicWidgetZones.SearchBox)
            return Content("");

        return View("~/Plugins/Misc.ArtificialIntelligence/Views/Public/AiSearchWidget.cshtml");
    }
}
