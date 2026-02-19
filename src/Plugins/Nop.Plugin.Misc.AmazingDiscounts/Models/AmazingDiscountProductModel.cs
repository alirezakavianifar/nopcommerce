using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.AmazingDiscounts.Models;

public record AmazingDiscountProductModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Misc.AmazingDiscounts.Fields.Product")]
    public int ProductId { get; set; }

    public string ProductName { get; set; }

    [NopResourceDisplayName("Plugins.Misc.AmazingDiscounts.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName("Plugins.Misc.AmazingDiscounts.Fields.CustomLabel")]
    public string CustomLabel { get; set; }

    [NopResourceDisplayName("Plugins.Misc.AmazingDiscounts.Fields.StartDateUtc")]
    public DateTime? StartDateUtc { get; set; }

    [NopResourceDisplayName("Plugins.Misc.AmazingDiscounts.Fields.EndDateUtc")]
    public DateTime? EndDateUtc { get; set; }
}
