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
        var langPattern = $"{{{NopRoutingDefaults.RouteValue.Language}:maxlength(2):{NopRoutingDefaults.LanguageParameterTransformer}=}}";

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

        // Localized customer/ routes with nopCommerce LanguageParameterTransformer
        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.Wallet.Customer.Lang",
            pattern: $"{langPattern}/customer/wallet",
            defaults: new { controller = "CustomerDashboard", action = "Wallet" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.Lottery.Customer.Lang",
            pattern: $"{langPattern}/customer/lottery",
            defaults: new { controller = "CustomerDashboard", action = "Lottery" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.LeaderGroups.Customer.Lang",
            pattern: $"{langPattern}/customer/leader-groups",
            defaults: new { controller = "CustomerDashboard", action = "LeaderGroups" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.SubgroupHistory.Customer.Lang",
            pattern: $"{langPattern}/customer/subgroup-history",
            defaults: new { controller = "CustomerDashboard", action = "SubgroupHistory" });

        // Localized group-purchase/ routes
        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.LeaderGroups.GP.Lang",
            pattern: $"{langPattern}/group-purchase/leader-groups",
            defaults: new { controller = "CustomerDashboard", action = "LeaderGroups" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.LeaderGroups.GP",
            pattern: "group-purchase/leader-groups",
            defaults: new { controller = "CustomerDashboard", action = "LeaderGroups" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.Wallet.GP.Lang",
            pattern: $"{langPattern}/group-purchase/wallet",
            defaults: new { controller = "CustomerDashboard", action = "Wallet" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.Wallet.GP",
            pattern: "group-purchase/wallet",
            defaults: new { controller = "CustomerDashboard", action = "Wallet" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.Lottery.GP.Lang",
            pattern: $"{langPattern}/group-purchase/lottery",
            defaults: new { controller = "CustomerDashboard", action = "Lottery" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.Lottery.GP",
            pattern: "group-purchase/lottery",
            defaults: new { controller = "CustomerDashboard", action = "Lottery" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.SubgroupHistory.GP.Lang",
            pattern: $"{langPattern}/group-purchase/subgroup-history",
            defaults: new { controller = "CustomerDashboard", action = "SubgroupHistory" });

        endpointRouteBuilder.MapControllerRoute(name: "Plugin.Misc.GroupPurchase.SubgroupHistory.GP",
            pattern: "group-purchase/subgroup-history",
            defaults: new { controller = "CustomerDashboard", action = "SubgroupHistory" });

        // Unlocalized customer/ routes
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
    public int Priority => 1000;
}
