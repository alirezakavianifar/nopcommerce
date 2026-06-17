using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using Nop.Plugin.Misc.GroupPurchase.Components;

namespace Nop.Plugin.Misc.GroupPurchase;

/// <summary>
/// Represents the group purchase plugin
/// </summary>
public class GroupPurchasePlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, IWidgetPlugin
{
    #region Fields

    protected readonly IWebHelper _webHelper;
    protected readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public GroupPurchasePlugin(IWebHelper webHelper,
        ILocalizationService localizationService)
    {
        _webHelper = webHelper;
        _localizationService = localizationService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/GroupPurchase/List";
    }

    /// <summary>
    /// Gets widget zones where this widget should be rendered
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the widget zones
    /// </returns>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> 
        { 
            PublicWidgetZones.OrderSummaryCartFooter,
            PublicWidgetZones.AccountNavigationAfter
        });
    }

    /// <summary>
    /// Gets a name of a view component for displaying widget
    /// </summary>
    /// <param name="widgetZone">Name of the widget zone</param>
    /// <returns>View component name</returns>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == PublicWidgetZones.AccountNavigationAfter)
            return typeof(CustomerDashboardNavigationViewComponent);

        return typeof(GroupPurchaseViewComponent);
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Misc.GroupPurchase.Manage"] = "Manage Group Purchases",
            ["Plugins.Misc.GroupPurchase.List.Title"] = "Group Purchases",
            ["Plugins.Misc.GroupPurchase.Fields.UniqueCode"] = "Unique Code",
            ["Plugins.Misc.GroupPurchase.Fields.LeaderCustomerId"] = "Leader Customer ID",
            ["Plugins.Misc.GroupPurchase.Fields.Status"] = "Status",
            ["Plugins.Misc.GroupPurchase.Fields.CreatedOnUtc"] = "Created On (UTC)",
            ["Plugins.Misc.GroupPurchase.Fields.DeliveryCity"] = "Delivery City",
            ["Plugins.Misc.GroupPurchase.Fields.DeliveryAddress"] = "Delivery Address",
            ["Plugins.Misc.GroupPurchase.SectionTitle"] = "Group Purchase",
            ["Plugins.Misc.GroupPurchase.SectionDescription"] = "Start a group purchase to share with friends and earn rewards!",
            ["Plugins.Misc.GroupPurchase.Button.Convert"] = "Start Group Purchase",
            ["Plugins.Misc.GroupPurchase.Button.Join"] = "Join Group",
            ["Plugins.Misc.GroupPurchase.RewardRule.Manage"] = "Manage Reward Rules",
            ["Plugins.Misc.GroupPurchase.RewardRule.AddNew"] = "Add New Reward Rule",
            ["Plugins.Misc.GroupPurchase.RewardRule.Edit"] = "Edit Reward Rule",
            ["Plugins.Misc.GroupPurchase.RewardRule.BackToList"] = "Back to list",
            ["Plugins.Misc.GroupPurchase.RewardRule.Fields.TargetRole"] = "Target Role",
            ["Plugins.Misc.GroupPurchase.RewardRule.Fields.RewardType"] = "Reward Type",
            ["Plugins.Misc.GroupPurchase.RewardRule.Fields.CalculationType"] = "Calculation Type",
            ["Plugins.Misc.GroupPurchase.RewardRule.Fields.Value"] = "Value",
            ["Plugins.Misc.GroupPurchase.RewardRule.Fields.CategoryId"] = "Category ID (0 for all)",
            ["Plugins.Misc.GroupPurchase.RewardRule.Fields.MinCartAmount"] = "Min Cart Amount",
            ["Plugins.Misc.GroupPurchase.RewardRule.Fields.MinMembers"] = "Min Members"
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.GroupPurchase");

        await base.UninstallAsync();
    }

    /// <summary>
    /// Manage admin menu
    /// </summary>
    /// <param name="rootNode">Root node</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task ManageSiteMapAsync(AdminMenuItem rootNode)
    {
        var menu = rootNode.GetItemBySystemName("Promotions");
        if (menu != null)
        {
            var pluginNode = new AdminMenuItem
            {
                SystemName = "Misc.GroupPurchase",
                Title = await _localizationService.GetResourceAsync("Plugins.Misc.GroupPurchase.Manage"),
                Url = "/Admin/GroupPurchase/List",
                IconClass = "far fa-dot-circle",
                Visible = true
            };
            menu.ChildNodes.Add(pluginNode);

            var rewardRuleNode = new AdminMenuItem
            {
                SystemName = "Misc.GroupPurchase.RewardRules",
                Title = await _localizationService.GetResourceAsync("Plugins.Misc.GroupPurchase.RewardRule.Manage"),
                Url = "/Admin/RewardRule/List",
                IconClass = "far fa-circle",
                Visible = true
            };
            pluginNode.ChildNodes.Add(rewardRuleNode);

            var walletNode = new AdminMenuItem
            {
                SystemName = "Misc.GroupPurchase.Wallets",
                Title = "Customer Wallets",
                Url = "/Admin/CustomerWallet/List",
                IconClass = "fas fa-wallet",
                Visible = true
            };
            pluginNode.ChildNodes.Add(walletNode);
        }
    }

    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the administration area
    /// </summary>
    public bool HideInWidgetList => false;

    #endregion
}
