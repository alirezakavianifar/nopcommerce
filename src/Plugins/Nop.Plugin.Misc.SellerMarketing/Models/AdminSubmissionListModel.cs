using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.SellerMarketing.Models;

public record AdminSubmissionListModel : BaseNopModel
{
    public IList<AdminSubmissionItemModel> Submissions { get; set; } = new List<AdminSubmissionItemModel>();
}

public record AdminSubmissionItemModel : BaseNopEntityModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; }
    public string Sku { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; }
    public string ImageUrl { get; set; }
    public DateTime SubmittedOnUtc { get; set; }
}
