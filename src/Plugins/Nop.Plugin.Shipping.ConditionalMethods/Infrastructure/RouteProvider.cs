using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Shipping.ConditionalMethods.Infrastructure;

/// <summary>
/// Registers admin routes for the Conditional Shipping Methods plugin
/// </summary>
public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Configure (global settings)
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Shipping.ConditionalMethods.Configure",
            pattern: "Admin/ConditionalShipping/Configure",
            defaults: new { controller = "ConditionalShippingAdmin", action = "Configure", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Shipping.ConditionalMethods.SaveConfigure",
            pattern: "Admin/ConditionalShipping/SaveConfigure",
            defaults: new { controller = "ConditionalShippingAdmin", action = "SaveConfigure", area = "Admin" });

        // City Mappings
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Shipping.ConditionalMethods.CityMappings",
            pattern: "Admin/ConditionalShipping/CityMappings",
            defaults: new { controller = "ConditionalShippingAdmin", action = "CityMappings", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Shipping.ConditionalMethods.CityMappingListData",
            pattern: "Admin/ConditionalShipping/CityMappingListData",
            defaults: new { controller = "ConditionalShippingAdmin", action = "CityMappingListData", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Shipping.ConditionalMethods.CreateCityMapping",
            pattern: "Admin/ConditionalShipping/CreateCityMapping",
            defaults: new { controller = "ConditionalShippingAdmin", action = "CreateCityMapping", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Shipping.ConditionalMethods.DeleteCityMapping",
            pattern: "Admin/ConditionalShipping/DeleteCityMapping",
            defaults: new { controller = "ConditionalShippingAdmin", action = "DeleteCityMapping", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Shipping.ConditionalMethods.EditCityMapping",
            pattern: "Admin/ConditionalShipping/EditCityMapping",
            defaults: new { controller = "ConditionalShippingAdmin", action = "EditCityMapping", area = "Admin" });

        // Product Mappings
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Shipping.ConditionalMethods.ProductMappings",
            pattern: "Admin/ConditionalShipping/ProductMappings",
            defaults: new { controller = "ConditionalShippingAdmin", action = "ProductMappings", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Shipping.ConditionalMethods.ProductMappingListData",
            pattern: "Admin/ConditionalShipping/ProductMappingListData",
            defaults: new { controller = "ConditionalShippingAdmin", action = "ProductMappingListData", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Shipping.ConditionalMethods.CreateProductMapping",
            pattern: "Admin/ConditionalShipping/CreateProductMapping",
            defaults: new { controller = "ConditionalShippingAdmin", action = "CreateProductMapping", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Shipping.ConditionalMethods.DeleteProductMapping",
            pattern: "Admin/ConditionalShipping/DeleteProductMapping",
            defaults: new { controller = "ConditionalShippingAdmin", action = "DeleteProductMapping", area = "Admin" });

        // Warehouse Mappings
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Shipping.ConditionalMethods.WarehouseMappings",
            pattern: "Admin/ConditionalShipping/WarehouseMappings",
            defaults: new { controller = "ConditionalShippingAdmin", action = "WarehouseMappings", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Shipping.ConditionalMethods.WarehouseMappingListData",
            pattern: "Admin/ConditionalShipping/WarehouseMappingListData",
            defaults: new { controller = "ConditionalShippingAdmin", action = "WarehouseMappingListData", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Shipping.ConditionalMethods.CreateWarehouseMapping",
            pattern: "Admin/ConditionalShipping/CreateWarehouseMapping",
            defaults: new { controller = "ConditionalShippingAdmin", action = "CreateWarehouseMapping", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Shipping.ConditionalMethods.DeleteWarehouseMapping",
            pattern: "Admin/ConditionalShipping/DeleteWarehouseMapping",
            defaults: new { controller = "ConditionalShippingAdmin", action = "DeleteWarehouseMapping", area = "Admin" });
    }

    public int Priority => 0;
}
