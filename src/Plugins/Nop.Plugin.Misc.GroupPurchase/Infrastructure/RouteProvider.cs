using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.GroupPurchase.Infrastructure;

/// <summary>
/// Represents plugin route provider
/// </summary>
public class RouteProvider : IRouteProvider
{
    /// <summary>
    /// Register routes
    /// </summary>
    /// <param name="endpointRouteBuilder">Route builder</param>
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Admin routes
        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.Admin.List",
            pattern: "Admin/GroupPurchase/List",
            defaults: new { controller = "GroupPurchaseAdmin", action = "List", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.Admin.ListData",
            pattern: "Admin/GroupPurchase/ListData",
            defaults: new { controller = "GroupPurchaseAdmin", action = "ListData", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.RewardRule.List",
            pattern: "Admin/RewardRule/List",
            defaults: new { controller = "RewardRule", action = "List", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.CustomerWallet.List",
            pattern: "Admin/CustomerWallet/List",
            defaults: new { controller = "CustomerWalletAdmin", action = "List", area = "Admin" });

        // Public routes
        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.Convert",
            pattern: "GroupPurchase/Convert",
            defaults: new { controller = "GroupPurchase", action = "ConvertToGroupPurchase" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.Join",
            pattern: "GroupPurchase/Join",
            defaults: new { controller = "GroupPurchase", action = "JoinGroupPurchase" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.Wallet",
            pattern: "customer/wallet",
            defaults: new { controller = "CustomerDashboard", action = "Wallet" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.Lottery",
            pattern: "customer/lottery",
            defaults: new { controller = "CustomerDashboard", action = "Lottery" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.LeaderGroups",
            pattern: "customer/leader-groups",
            defaults: new { controller = "CustomerDashboard", action = "LeaderGroups" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.SubgroupHistory",
            pattern: "customer/subgroup-history",
            defaults: new { controller = "CustomerDashboard", action = "SubgroupHistory" });
    }

    /// <summary>
    /// Gets a priority of route provider
    /// </summary>
    public int Priority => 0;
}
