using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.MultiFactorAuth.SMS.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.MultiFactorAuth.SMS.Components;

public class SMSVerificationViewComponent : NopViewComponent
{
    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
        var model = new SMSTokenModel();
        return View("~/Plugins/MultiFactorAuth.SMS/Views/Customer/SMSVerification.cshtml", model);
    }
}
