using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.AmazingDiscounts;

public class AmazingDiscountsPlugin : BasePlugin, IMiscPlugin, IWidgetPlugin, IAdminMenuPlugin
{
    protected readonly IWebHelper _webHelper;
    protected readonly ILocalizationService _localizationService;

    public AmazingDiscountsPlugin(IWebHelper webHelper,
        ILocalizationService localizationService)
    {
        _webHelper = webHelper;
        _localizationService = localizationService;
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/AmazingDiscounts/List";
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.Footer });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(Components.AmazingDiscountsFooterViewComponent);
    }

    public override async Task InstallAsync()
    {
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Misc.AmazingDiscounts.PageTitle"] = "Amazing Discounts",
            ["Plugins.Misc.AmazingDiscounts.FooterLink"] = "Amazing Discounts",
            ["Plugins.Misc.AmazingDiscounts.Fields.Product"] = "Product",
            ["Plugins.Misc.AmazingDiscounts.Fields.DisplayOrder"] = "Display order",
            ["Plugins.Misc.AmazingDiscounts.Fields.CustomLabel"] = "Custom label",
            ["Plugins.Misc.AmazingDiscounts.Fields.StartDateUtc"] = "Start Date (UTC)",
            ["Plugins.Misc.AmazingDiscounts.Fields.EndDateUtc"] = "End Date (UTC)"
        });

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.AmazingDiscounts");

        await base.UninstallAsync();
    }

    public async Task ManageSiteMapAsync(AdminMenuItem rootNode)
    {
        var menu = rootNode.GetItemBySystemName("Catalog");
        if (menu != null)
        {
            menu.ChildNodes.Add(new AdminMenuItem
            {
                SystemName = "Misc.AmazingDiscounts",
                Title = "Amazing Discounts",
                Url = "/Admin/AmazingDiscounts/List",
                IconClass = "far fa-dot-circle",
                Visible = true
            });
        }
    }

    public bool HideInWidgetList => false;
}
