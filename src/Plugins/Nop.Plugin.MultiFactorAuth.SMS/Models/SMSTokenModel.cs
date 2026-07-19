using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.MultiFactorAuth.SMS.Models;

public record SMSTokenModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.MultiFactorAuth.SMS.Customer.VerificationCode")]
    public string Token { get; set; }
}
