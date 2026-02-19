using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.AmazingDiscounts.Services;
using Nop.Services.Catalog;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.AmazingDiscounts.Controllers;

[Route("api/amazing-discounts")]
public class AmazingDiscountApiController : BasePluginController
{
    protected readonly IAmazingDiscountService _amazingDiscountService;
    protected readonly IProductService _productService;

    public AmazingDiscountApiController(IAmazingDiscountService amazingDiscountService,
        IProductService productService)
    {
        _amazingDiscountService = amazingDiscountService;
        _productService = productService;
    }

    [HttpGet]
    public virtual async Task<IActionResult> GetAmazingDiscounts()
    {
        var activeAmazingProducts = await _amazingDiscountService.GetActiveAmazingDiscountProductsAsync();
        var productIds = activeAmazingProducts.Select(p => p.ProductId).ToList();
        
        var products = await _productService.GetProductsByIdsAsync(productIds.ToArray());
        
        var model = activeAmazingProducts.Select(ap => {
            var product = products.FirstOrDefault(p => p.Id == ap.ProductId);
            return new {
                ap.Id,
                ap.ProductId,
                ProductName = product?.Name,
                ap.CustomLabel,
                ap.StartDateUtc,
                ap.EndDateUtc,
                Price = product?.Price
            };
        });

        return Ok(model);
    }
}
