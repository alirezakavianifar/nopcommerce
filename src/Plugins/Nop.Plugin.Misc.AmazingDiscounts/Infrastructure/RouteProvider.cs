using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace Nop.Plugin.Misc.AmazingDiscounts.Infrastructure;

public class RouteProvider : BaseRouteProvider, IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.AmazingDiscounts.List.Lang",
            pattern: "{lang:maxlength(2)}/amazing-discounts",
            defaults: new { controller = "AmazingDiscount", action = "List" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.AmazingDiscounts.List",
            pattern: "amazing-discounts",
            defaults: new { controller = "AmazingDiscount", action = "List" });
    }

    public int Priority => 1000;
}
