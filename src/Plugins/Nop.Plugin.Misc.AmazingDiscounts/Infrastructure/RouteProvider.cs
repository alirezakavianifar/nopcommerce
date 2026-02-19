using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.AmazingDiscounts.Infrastructure;

public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.AmazingDiscounts.List",
            pattern: "amazing-discounts",
            defaults: new { controller = "AmazingDiscount", action = "List" });
    }

    public int Priority => 0;
}
