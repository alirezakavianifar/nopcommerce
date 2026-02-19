using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Misc.AmazingDiscounts.Models;

public record AmazingDiscountPublicModel
{
    public IList<Product> Products { get; set; } = new List<Product>();
}
