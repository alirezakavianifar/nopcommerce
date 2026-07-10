using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Components;

public class AiChatbotWidgetViewComponent : NopViewComponent
{
    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
        if (widgetZone != PublicWidgetZones.BodyEndHtmlTagBefore)
            return Content("");

        return View("~/Plugins/Misc.ArtificialIntelligence/Views/Public/AiChatbotWidget.cshtml");
    }
}
