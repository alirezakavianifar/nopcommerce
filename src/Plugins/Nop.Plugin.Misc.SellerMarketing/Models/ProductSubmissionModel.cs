using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.SellerMarketing.Models;

public record ProductSubmissionModel : BaseNopEntityModel
{
    public ProductSubmissionModel()
    {
        AvailableCategories = new List<SelectListItem>();
    }

    [Required]
    [NopResourceDisplayName("Plugins.Misc.SellerMarketing.Dashboard.ProductName")]
    public string Name { get; set; }

    [Required]
    [NopResourceDisplayName("Plugins.Misc.SellerMarketing.Dashboard.SKU")]
    public string Sku { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SellerMarketing.Dashboard.ShortDescription")]
    public string ShortDescription { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SellerMarketing.Dashboard.FullDescription")]
    public string FullDescription { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SellerMarketing.Dashboard.Price")]
    public decimal Price { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SellerMarketing.Dashboard.Stock")]
    public int StockQuantity { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.Categories")]
    public int SelectedCategoryId { get; set; }

    public IList<SelectListItem> AvailableCategories { get; set; }

    public string ImageUrl { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SellerMarketing.Dashboard.ImageFile")]
    public IFormFile ImageFile { get; set; }
}
