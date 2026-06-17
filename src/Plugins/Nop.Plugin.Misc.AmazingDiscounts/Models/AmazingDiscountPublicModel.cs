using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Misc.AmazingDiscounts.Models;

public record AmazingDiscountPublicModel
{
    public IList<AmazingDiscountProductItemModel> Products { get; set; } = new List<AmazingDiscountProductItemModel>();
}

public record AmazingDiscountProductItemModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string SeName { get; set; }
    public string ShortDescription { get; set; }
    public string PictureUrl { get; set; }
    public string OldPrice { get; set; }
    public string Price { get; set; }
    public int DiscountPercentage { get; set; }
    public string CustomLabel { get; set; }
}
