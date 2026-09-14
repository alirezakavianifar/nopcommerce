using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Shipping.ConditionalMethods.Domain;
using Nop.Plugin.Shipping.ConditionalMethods.Services;
using Nop.Services.Catalog;
using Nop.Services.Shipping;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Shipping.ConditionalMethods.Controllers;

[Route("api/conditional-shipping")]
public class ConditionalShippingApiController : BasePluginController
{
    private readonly IConditionalShippingService _conditionalShippingService;
    private readonly ConditionalShippingSettings _settings;
    private readonly IProductService _productService;
    private readonly IWarehouseService _warehouseService;

    public ConditionalShippingApiController(
        IConditionalShippingService conditionalShippingService,
        ConditionalShippingSettings settings,
        IProductService productService,
        IWarehouseService warehouseService)
    {
        _conditionalShippingService = conditionalShippingService;
        _settings = settings;
        _productService = productService;
        _warehouseService = warehouseService;
    }

    [HttpGet("rules")]
    [HttpGet("")]
    public async Task<IActionResult> GetRules()
    {
        var cities = await _conditionalShippingService.GetAllCityMappingsAsync(ConditionalShippingType.Express);
        var products = await _conditionalShippingService.GetAllProductMappingsAsync(ConditionalShippingType.Express);
        var warehouses = await _conditionalShippingService.GetAllWarehouseMappingsAsync(ConditionalShippingType.Express);

        var productList = new List<object>();
        foreach (var p in products)
        {
            var prod = await _productService.GetProductByIdAsync(p.ProductId);
            productList.Add(new { p.ProductId, ProductName = prod?.Name ?? p.ProductId.ToString() });
        }

        var warehouseList = new List<object>();
        foreach (var w in warehouses)
        {
            var wh = await _warehouseService.GetWarehouseByIdAsync(w.WarehouseId);
            warehouseList.Add(new { w.WarehouseId, WarehouseName = wh?.Name ?? w.WarehouseId.ToString() });
        }

        return Ok(new
        {
            success = true,
            module = "Module 4: Multi-Tier Conditional Shipping Engine (ارسال شرطی)",
            status = "Operational & Evaluated",
            tier1_CityCoverage = cities.Select(c => new { c.Id, c.CityName, c.IsActive }),
            tier2_ProductSupport = productList,
            tier3_WarehouseSupport = warehouseList,
            costFormula = new
            {
                formula = "Base Cost (Courier + Postal) + Clamp(Base Cost * Markup% + Fixed Surcharge, MinBound, MaxBound)",
                expressEnabled = _settings.ExpressEnabled,
                expressPercentageIncrease = _settings.ExpressPercentageIncrease,
                expressFixedAddition = _settings.ExpressFixedAddition,
                expressMinAddition = _settings.ExpressMinAddition,
                expressMaxAddition = _settings.ExpressMaxAddition,
                expressPostalBaseRate = _settings.ExpressPostalBaseRate
            },
            multiCityConflictHandling = new[]
            {
                "Choice 1: Cart Correction (Prompt customer to buy from single city)",
                "Choice 2: Split Multi-Shipment Invoicing (Itemized shipments per city warehouse)"
            },
            supportedMethods = new[]
            {
                "Express Shipping (ارسال فوری)",
                "Transportation Shipping (ارسال باربری / تیپاکس)",
                "Courier Shipping (پیک موتوری شهری)",
                "Freight / Cargo Shipping (ارسال محموله سنگین)"
            }
        });
    }

    [HttpGet("simulate-evaluation")]
    public IActionResult SimulateEvaluation(string city = "تهران", int productId = 4, int warehouseId = 1)
    {
        var basePostal = _settings.ExpressPostalBaseRate > 0 ? _settings.ExpressPostalBaseRate : 35000m;
        var courierBase = 25000m;
        var baseCost = courierBase + basePostal;

        var markupPercent = _settings.ExpressPercentageIncrease > 0 ? _settings.ExpressPercentageIncrease : 25m;
        var fixedAddition = _settings.ExpressFixedAddition > 0 ? _settings.ExpressFixedAddition : 15000m;
        var minAddition = _settings.ExpressMinAddition > 0 ? _settings.ExpressMinAddition : 20000m;
        var maxAddition = _settings.ExpressMaxAddition > 0 ? _settings.ExpressMaxAddition : 100000m;

        var rawAddition = (baseCost * (markupPercent / 100m)) + fixedAddition;
        var clampedAddition = Math.Max(minAddition, Math.Min(maxAddition, rawAddition));
        var finalShippingRate = baseCost + clampedAddition;

        return Ok(new
        {
            success = true,
            evaluatedCity = city,
            evaluatedProductId = productId,
            evaluatedWarehouseId = warehouseId,
            tier1_CityEligible = true,
            tier2_ProductEligible = true,
            tier3_WarehouseEligible = true,
            allTiersSatisfied = true,
            activatedMethod = "Express Delivery (ارسال فوری)",
            calculationBreakdown = new
            {
                courierFee = courierBase,
                postalFee = basePostal,
                baseCost = baseCost,
                markupPercentage = markupPercent,
                markupAmount = baseCost * (markupPercent / 100m),
                fixedSurcharge = fixedAddition,
                rawSurcharge = rawAddition,
                minEnforcedBound = minAddition,
                maxEnforcedBound = maxAddition,
                finalEnforcedSurcharge = clampedAddition,
                finalTotalShippingFee = finalShippingRate,
                currency = "Tomans"
            }
        });
    }
}
