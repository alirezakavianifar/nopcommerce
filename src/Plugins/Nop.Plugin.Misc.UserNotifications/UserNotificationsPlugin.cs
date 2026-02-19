using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using Nop.Plugin.Misc.UserNotifications.Components;

namespace Nop.Plugin.Misc.UserNotifications;

/// <summary>
/// Represents the user notifications plugin
/// </summary>
public class UserNotificationsPlugin : BasePlugin, IMiscPlugin, IWidgetPlugin, IAdminMenuPlugin
{
    #region Fields

    protected readonly IWebHelper _webHelper;
    protected readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public UserNotificationsPlugin(IWebHelper webHelper,
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
        return $"{_webHelper.GetStoreLocation()}Admin/UserNotifications/List";
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
        return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.HomepageTop, PublicWidgetZones.HeaderBefore });
    }

    /// <summary>
    /// Gets a name of a view component for displaying widget
    /// </summary>
    /// <param name="widgetZone">Name of the widget zone</param>
    /// <returns>View component name</returns>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(UserNotificationsViewComponent);
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Misc.UserNotifications.Announcements"] = "System Announcements",
            ["Plugins.Misc.UserNotifications.AddAnnouncement"] = "Add a new announcement",
            ["Plugins.Misc.UserNotifications.EditAnnouncement"] = "Edit announcement",
            ["Plugins.Misc.UserNotifications.BackToList"] = "back to announcement list",
            ["Plugins.Misc.UserNotifications.Fields.Title"] = "Title",
            ["Plugins.Misc.UserNotifications.Fields.Body"] = "Body",
            ["Plugins.Misc.UserNotifications.Fields.StartDateUtc"] = "Start Date (UTC)",
            ["Plugins.Misc.UserNotifications.Fields.EndDateUtc"] = "End Date (UTC)",
            ["Plugins.Misc.UserNotifications.Fields.IsPublished"] = "Is published",
            ["Plugins.Misc.UserNotifications.Added"] = "The announcement has been added successfully.",
            ["Plugins.Misc.UserNotifications.Updated"] = "The announcement has been updated successfully.",
            ["Plugins.Misc.UserNotifications.Deleted"] = "The announcement has been deleted successfully."
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.UserNotifications");

        await base.UninstallAsync();
    }

    #endregion

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
            menu.ChildNodes.Add(new AdminMenuItem
            {
                SystemName = "Misc.UserNotifications",
                Title = await _localizationService.GetResourceAsync("Plugins.Misc.UserNotifications.Announcements"),
                Url = "/Admin/UserNotifications/List",
                IconClass = "far fa-dot-circle",
                Visible = true
            });
        }
    }

    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the administration area
    /// </summary>
    public bool HideInWidgetList => false;
}
