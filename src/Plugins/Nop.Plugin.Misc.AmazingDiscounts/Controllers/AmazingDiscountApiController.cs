using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.AmazingDiscounts.Services;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.AmazingDiscounts.Controllers;

[Route("api/amazing-discounts")]
public class AmazingDiscountApiController : BasePluginController
{
    protected readonly IAmazingDiscountService _amazingDiscountService;
    protected readonly IProductService _productService;
    protected readonly IPictureService _pictureService;
    protected readonly IPriceCalculationService _priceCalculationService;
    protected readonly IPriceFormatter _priceFormatter;
    protected readonly IWorkContext _workContext;
    protected readonly IStoreContext _storeContext;

    public AmazingDiscountApiController(IAmazingDiscountService amazingDiscountService,
        IProductService productService,
        IPictureService pictureService,
        IPriceCalculationService priceCalculationService,
        IPriceFormatter priceFormatter,
        IWorkContext workContext,
        IStoreContext storeContext)
    {
        _amazingDiscountService = amazingDiscountService;
        _productService = productService;
        _pictureService = pictureService;
        _priceCalculationService = priceCalculationService;
        _priceFormatter = priceFormatter;
        _workContext = workContext;
        _storeContext = storeContext;
    }

    [HttpGet]
    public virtual async Task<IActionResult> GetAmazingDiscounts()
    {
        var activeAmazingProducts = await _amazingDiscountService.GetActiveAmazingDiscountProductsAsync();
        var productIds = activeAmazingProducts.Select(p => p.ProductId).ToArray();
        var products = await _productService.GetProductsByIdsAsync(productIds);
        
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var list = new List<object>();

        foreach (var ap in activeAmazingProducts)
        {
            var product = products.FirstOrDefault(p => p.Id == ap.ProductId);
            if (product == null)
                continue;

            // Get Picture Url
            var picture = await _pictureService.GetProductPictureAsync(product, null);
            var pictureUrl = picture != null 
                ? (await _pictureService.GetPictureUrlAsync(picture, 400)).Url 
                : await _pictureService.GetDefaultPictureUrlAsync(400);

            // Calculate final price & discounts
            var (priceWithoutDiscounts, finalPrice, _, _) = 
                await _priceCalculationService.GetFinalPriceAsync(product, customer, store);

            var oldPriceVal = product.OldPrice;
            if (oldPriceVal == 0 && priceWithoutDiscounts > finalPrice)
                oldPriceVal = priceWithoutDiscounts;

            int discountPercentage = 0;
            string oldPriceStr = null;
            if (oldPriceVal > finalPrice)
            {
                oldPriceStr = await _priceFormatter.FormatPriceAsync(oldPriceVal);
                discountPercentage = (int)Math.Round((oldPriceVal - finalPrice) / oldPriceVal * 100);
            }
            var priceStr = await _priceFormatter.FormatPriceAsync(finalPrice);

            // Duration
            var remainingSeconds = ap.EndDateUtc.HasValue 
                ? Math.Max(0, (long)(ap.EndDateUtc.Value - DateTime.UtcNow).TotalSeconds)
                : (long)(86400 * 2 + 3600 * 4);

            var timeSpan = TimeSpan.FromSeconds(remainingSeconds);
            var formattedRemaining = $"{timeSpan.Days}d {timeSpan.Hours:D2}h {timeSpan.Minutes:D2}m {timeSpan.Seconds:D2}s";

            var claimedPercentage = 65 + ((product.Id * 11) % 24);
            var stockQuantity = product.StockQuantity > 0 ? Math.Min(product.StockQuantity, 12) : 4;

            list.Add(new
            {
                id = ap.Id,
                productId = ap.ProductId,
                productName = product.Name,
                customLabel = ap.CustomLabel ?? "پیشنهاد شگفت‌انگیز",
                oldPrice = oldPriceStr,
                price = priceStr,
                discountPercentage = discountPercentage,
                pictureUrl = pictureUrl,
                startDateUtc = ap.StartDateUtc,
                endDateUtc = ap.EndDateUtc,
                remainingSeconds = remainingSeconds,
                formattedRemaining = formattedRemaining,
                stockQuantity = stockQuantity,
                claimedPercentage = claimedPercentage,
                isAvailable = stockQuantity > 0
            });
        }

        return Ok(list);
    }
}
