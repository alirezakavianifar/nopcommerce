using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.MultiFactorAuth.SMS.Models;

public record SMSAuthModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.MultiFactorAuth.SMS.Customer.PhoneNumber")]
    public string PhoneNumber { get; set; }

    [NopResourceDisplayName("Plugins.MultiFactorAuth.SMS.Customer.VerificationCode")]
    public string Code { get; set; }

    public bool IsSMS2FAActive { get; set; }
}
