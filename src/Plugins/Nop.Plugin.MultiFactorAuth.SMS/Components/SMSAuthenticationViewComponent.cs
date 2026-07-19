using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.MultiFactorAuth.SMS.Models;
using Nop.Services.Common;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.MultiFactorAuth.SMS.Components;

public class SMSAuthenticationViewComponent : NopViewComponent
{
    protected readonly IWorkContext _workContext;
    protected readonly IGenericAttributeService _genericAttributeService;

    public SMSAuthenticationViewComponent(
        IWorkContext workContext,
        IGenericAttributeService genericAttributeService)
    {
        _workContext = workContext;
        _genericAttributeService = genericAttributeService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var isSMSActive = await _genericAttributeService.GetAttributeAsync<bool>(customer, SMSDefaults.SMS2FAEnabledAttribute);
        var phoneNumber = await _genericAttributeService.GetAttributeAsync<string>(customer, SMSDefaults.SMS2FAPhoneNumberAttribute);

        if (string.IsNullOrEmpty(phoneNumber))
            phoneNumber = customer?.Phone;

        var model = new SMSAuthModel
        {
            PhoneNumber = phoneNumber,
            IsSMS2FAActive = isSMSActive
        };

        return View("~/Plugins/MultiFactorAuth.SMS/Views/Customer/SMSAuthentication.cshtml", model);
    }
}
