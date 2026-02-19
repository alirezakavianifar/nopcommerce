using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.AmazingDiscounts.Models;
using Nop.Plugin.Misc.AmazingDiscounts.Services;
using Nop.Services.Catalog;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.AmazingDiscounts.Controllers;

public class AmazingDiscountController : BasePluginController
{
    protected readonly IAmazingDiscountService _amazingDiscountService;
    protected readonly IProductService _productService;

    public AmazingDiscountController(IAmazingDiscountService amazingDiscountService,
        IProductService productService)
    {
        _amazingDiscountService = amazingDiscountService;
        _productService = productService;
    }

    public virtual async Task<IActionResult> List()
    {
        var activeAmazingProducts = await _amazingDiscountService.GetActiveAmazingDiscountProductsAsync();
        var productIds = activeAmazingProducts.Select(p => p.ProductId).ToList();
        
        var products = await _productService.GetProductsByIdsAsync(productIds.ToArray());
        
        // Sort by the display order in our plugin
        var sortedProducts = productIds
            .Select(id => products.FirstOrDefault(p => p.Id == id))
            .Where(p => p != null)
            .ToList();

        var model = new AmazingDiscountPublicModel
        {
            Products = sortedProducts
        };

        return View("~/Plugins/Misc.AmazingDiscounts/Views/Public/List.cshtml", model);
    }
}
