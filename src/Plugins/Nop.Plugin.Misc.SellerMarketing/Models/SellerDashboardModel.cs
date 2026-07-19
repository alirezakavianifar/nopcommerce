using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.SellerMarketing.Models;

public record SellerDashboardModel : BaseNopModel
{
    public IList<ProductSubmissionItemModel> Products { get; set; } = new List<ProductSubmissionItemModel>();
}

public record ProductSubmissionItemModel : BaseNopEntityModel
{
    public string Name { get; set; }
    public string Sku { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string ImageUrl { get; set; }
    public string Status { get; set; }
    public string AdminComment { get; set; }
    public DateTime SubmittedOnUtc { get; set; }
}
