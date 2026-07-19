using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.SellerMarketing.Infrastructure;

public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Public Seller Dashboard Routes
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Misc.SellerMarketing.Public.Dashboard",
            pattern: "seller/dashboard",
            defaults: new { controller = "SellerMarketingPublic", action = "Dashboard" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Misc.SellerMarketing.Public.AddProduct",
            pattern: "seller/product/add",
            defaults: new { controller = "SellerMarketingPublic", action = "AddProduct" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Misc.SellerMarketing.Public.EditProduct",
            pattern: "seller/product/edit/{id:int}",
            defaults: new { controller = "SellerMarketingPublic", action = "EditProduct" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Misc.SellerMarketing.Public.DeleteProduct",
            pattern: "seller/product/delete/{id:int}",
            defaults: new { controller = "SellerMarketingPublic", action = "DeleteProduct" });

        // Admin Review Routes
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Misc.SellerMarketing.Admin.List",
            pattern: "Admin/SellerMarketing/List",
            defaults: new { controller = "SellerMarketingAdmin", action = "List", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Misc.SellerMarketing.Admin.Approve",
            pattern: "Admin/SellerMarketing/Approve",
            defaults: new { controller = "SellerMarketingAdmin", action = "Approve", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Misc.SellerMarketing.Admin.Reject",
            pattern: "Admin/SellerMarketing/Reject",
            defaults: new { controller = "SellerMarketingAdmin", action = "Reject", area = "Admin" });
    }

    public int Priority => 0;
}
