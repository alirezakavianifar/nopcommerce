using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.MultiFactorAuth.SMS.Infrastructure;

/// <summary>
/// Represents plugin route provider
/// </summary>
public class RouteProvider : IRouteProvider
{
    /// <summary>
    /// Register routes
    /// </summary>
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Admin config route
        endpointRouteBuilder.MapControllerRoute(
            name: SMSDefaults.ConfigurationRouteName,
            pattern: "Plugins/SMS/Configure",
            defaults: new { controller = "SMSAuthenticationAdmin", action = "Configure", area = AreaNames.ADMIN });
    }

    /// <summary>
    /// Gets a priority of route provider
    /// </summary>
    public int Priority => 0;
}
