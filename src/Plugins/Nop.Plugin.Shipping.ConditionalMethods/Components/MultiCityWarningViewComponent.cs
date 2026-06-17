using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Shipping.ConditionalMethods.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Shipping.ConditionalMethods.Components;

/// <summary>
/// Widget rendered at the top of the shipping method step that warns the customer
/// when their cart contains products from warehouses located in different cities.
/// </summary>
public class MultiCityWarningViewComponent : NopViewComponent
{
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IProductService _productService;
    private readonly IWarehouseService _warehouseService;
    private readonly IAddressService _addressService;

    public MultiCityWarningViewComponent(
        IWorkContext workContext,
        IStoreContext storeContext,
        IShoppingCartService shoppingCartService,
        IProductService productService,
        IWarehouseService warehouseService,
        IAddressService addressService)
    {
        _workContext = workContext;
        _storeContext = storeContext;
        _shoppingCartService = shoppingCartService;
        _productService = productService;
        _warehouseService = warehouseService;
        _addressService = addressService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var model = new MultiCityWarningModel();

        if (widgetZone != PublicWidgetZones.CheckoutShippingMethodTop)
            return View("~/Plugins/Shipping.ConditionalMethods/Views/Public/MultiCityWarning.cshtml", model);

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var cartItems = await _shoppingCartService.GetShoppingCartAsync(
            customer,
            ShoppingCartType.ShoppingCart,
            store.Id);

        if (!cartItems.Any())
            return View("~/Plugins/Shipping.ConditionalMethods/Views/Public/MultiCityWarning.cshtml", model);

        var citiesFound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in cartItems)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            if (product == null || !product.IsShipEnabled)
                continue;

            // Resolve warehouse: use product's default WarehouseId
            if (product.WarehouseId > 0)
            {
                var warehouse = await _warehouseService.GetWarehouseByIdAsync(product.WarehouseId);
                if (warehouse != null)
                {
                    var address = await _addressService.GetAddressByIdAsync(warehouse.AddressId);
                    if (address != null && !string.IsNullOrWhiteSpace(address.City))
                        citiesFound.Add(address.City.Trim());
                }
            }
        }

        if (citiesFound.Count > 1)
        {
            model.HasMultipleCities = true;
            model.Cities = citiesFound.ToList();
        }

        return View("~/Plugins/Shipping.ConditionalMethods/Views/Public/MultiCityWarning.cshtml", model);
    }
}
