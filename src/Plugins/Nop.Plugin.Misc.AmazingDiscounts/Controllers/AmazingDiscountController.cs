using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.AmazingDiscounts.Models;
using Nop.Plugin.Misc.AmazingDiscounts.Services;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.AmazingDiscounts.Controllers;

public class AmazingDiscountController : BasePluginController
{
    protected readonly IAmazingDiscountService _amazingDiscountService;
    protected readonly IProductService _productService;
    protected readonly IPictureService _pictureService;
    protected readonly IPriceCalculationService _priceCalculationService;
    protected readonly IPriceFormatter _priceFormatter;
    protected readonly IWorkContext _workContext;
    protected readonly IStoreContext _storeContext;
    protected readonly IUrlRecordService _urlRecordService;

    public AmazingDiscountController(IAmazingDiscountService amazingDiscountService,
        IProductService productService,
        IPictureService pictureService,
        IPriceCalculationService priceCalculationService,
        IPriceFormatter priceFormatter,
        IWorkContext workContext,
        IStoreContext storeContext,
        IUrlRecordService urlRecordService)
    {
        _amazingDiscountService = amazingDiscountService;
        _productService = productService;
        _pictureService = pictureService;
        _priceCalculationService = priceCalculationService;
        _priceFormatter = priceFormatter;
        _workContext = workContext;
        _storeContext = storeContext;
        _urlRecordService = urlRecordService;
    }

    public virtual async Task<IActionResult> List()
    {
        var activeAmazingProducts = await _amazingDiscountService.GetActiveAmazingDiscountProductsAsync();
        var productIds = activeAmazingProducts.Select(p => p.ProductId).ToArray();
        
        var products = await _productService.GetProductsByIdsAsync(productIds);
        
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        
        var productModels = new List<AmazingDiscountProductItemModel>();
        foreach (var activeProd in activeAmazingProducts)
        {
            var product = products.FirstOrDefault(p => p.Id == activeProd.ProductId);
            if (product == null) continue;
            
            var seName = await _urlRecordService.GetSeNameAsync(product);
            
            // Get picture
            var picture = await _pictureService.GetProductPictureAsync(product, null);
            var pictureUrl = picture != null 
                ? (await _pictureService.GetPictureUrlAsync(picture, 300)).Url 
                : await _pictureService.GetDefaultPictureUrlAsync(300);
            
            // Calculate price
            var (priceWithoutDiscounts, finalPrice, appliedDiscountAmount, appliedDiscounts) = 
                await _priceCalculationService.GetFinalPriceAsync(product, customer, store);
                
            // Format price using IPriceFormatter
            var priceStr = await _priceFormatter.FormatPriceAsync(finalPrice);
            
            // Calculate if there's an old price/discount.
            var oldPriceVal = product.OldPrice;
            if (oldPriceVal == 0 && priceWithoutDiscounts > finalPrice)
            {
                oldPriceVal = priceWithoutDiscounts;
            }
            
            string oldPriceStr = null;
            int discountPercentage = 0;
            if (oldPriceVal > finalPrice)
            {
                oldPriceStr = await _priceFormatter.FormatPriceAsync(oldPriceVal);
                discountPercentage = (int)Math.Round((oldPriceVal - finalPrice) / oldPriceVal * 100);
            }
            
            productModels.Add(new AmazingDiscountProductItemModel
            {
                Id = product.Id,
                Name = product.Name,
                SeName = seName,
                ShortDescription = product.ShortDescription,
                PictureUrl = pictureUrl,
                OldPrice = oldPriceStr,
                Price = priceStr,
                DiscountPercentage = discountPercentage,
                CustomLabel = activeProd.CustomLabel
            });
        }

        var model = new AmazingDiscountPublicModel
        {
            Products = productModels
        };

        return View("~/Plugins/Misc.AmazingDiscounts/Views/Public/List.cshtml", model);
    }
}
