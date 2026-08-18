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
    protected readonly ILanguageService _languageService;
    protected readonly IWorkContext _workContext;

    private static bool _resourcesEnsured = false;

    public AmazingDiscountsPlugin(IWebHelper webHelper,
        ILocalizationService localizationService,
        ILanguageService languageService,
        IWorkContext workContext)
    {
        _webHelper = webHelper;
        _localizationService = localizationService;
        _languageService = languageService;
        _workContext = workContext;
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

    private async Task EnsureLocaleResourcesAsync()
    {
        try
        {
            var languages = await _languageService.GetAllLanguagesAsync();
            var enLang = languages.FirstOrDefault(l => l.LanguageCulture.StartsWith("en", StringComparison.OrdinalIgnoreCase));
            var faLang = languages.FirstOrDefault(l => l.LanguageCulture.StartsWith("fa", StringComparison.OrdinalIgnoreCase));

            var enResources = new Dictionary<string, string>
            {
                ["Plugins.Misc.AmazingDiscounts.Manage"] = "Amazing Discounts",
                ["Plugins.Misc.AmazingDiscounts.PageTitle"] = "Amazing Discounts",
                ["Plugins.Misc.AmazingDiscounts.FooterLink"] = "Amazing Discounts",
                ["Plugins.Misc.AmazingDiscounts.HeroSubtitle"] = "Unbeatable deals on top products. Grab them before they're gone!",
                ["Plugins.Misc.AmazingDiscounts.HotOffer"] = "Hot Offer",
                ["Plugins.Misc.AmazingDiscounts.ViewDeal"] = "View Deal",
                ["Plugins.Misc.AmazingDiscounts.EmptyList"] = "No amazing discounts at the moment. Check back soon for exclusive promotions!",
                ["Plugins.Misc.AmazingDiscounts.Fields.Product"] = "Product",
                ["Plugins.Misc.AmazingDiscounts.Fields.DisplayOrder"] = "Display order",
                ["Plugins.Misc.AmazingDiscounts.Fields.CustomLabel"] = "Custom label",
                ["Plugins.Misc.AmazingDiscounts.Fields.StartDateUtc"] = "Start Date (UTC)",
                ["Plugins.Misc.AmazingDiscounts.Fields.EndDateUtc"] = "End Date (UTC)"
            };

            var faResources = new Dictionary<string, string>
            {
                ["Plugins.Misc.AmazingDiscounts.Manage"] = "تخفیف‌های شگفت‌انگیز",
                ["Plugins.Misc.AmazingDiscounts.PageTitle"] = "تخفیف‌های شگفت‌انگیز",
                ["Plugins.Misc.AmazingDiscounts.FooterLink"] = "تخفیف‌های شگفت‌انگیز",
                ["Plugins.Misc.AmazingDiscounts.HeroSubtitle"] = "تخفیف‌های بی‌نظیر روی برترین کالاها. قبل از اتمام فرصت خرید کنید!",
                ["Plugins.Misc.AmazingDiscounts.HotOffer"] = "پیشنهاد ویژه",
                ["Plugins.Misc.AmazingDiscounts.ViewDeal"] = "مشاهده و خرید",
                ["Plugins.Misc.AmazingDiscounts.EmptyList"] = "در حال حاضر هیچ تخفیف شگفت‌انگیزی وجود ندارد. به زودی سر بزنید!",
                ["Plugins.Misc.AmazingDiscounts.Fields.Product"] = "محصول",
                ["Plugins.Misc.AmazingDiscounts.Fields.DisplayOrder"] = "ترتیب نمایش",
                ["Plugins.Misc.AmazingDiscounts.Fields.CustomLabel"] = "برچسب سفارشی",
                ["Plugins.Misc.AmazingDiscounts.Fields.StartDateUtc"] = "تاریخ شروع (UTC)",
                ["Plugins.Misc.AmazingDiscounts.Fields.EndDateUtc"] = "تاریخ پایان (UTC)"
            };

            if (enLang != null)
                await _localizationService.AddOrUpdateLocaleResourceAsync(enResources, enLang.Id);
            if (faLang != null)
                await _localizationService.AddOrUpdateLocaleResourceAsync(faResources, faLang.Id);

            await _localizationService.AddOrUpdateLocaleResourceAsync(faResources);
            _resourcesEnsured = true;
        }
        catch
        {
            // Ignore during startup/install transient state
        }
    }

    public override async Task InstallAsync()
    {
        await EnsureLocaleResourcesAsync();
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.AmazingDiscounts");

        await base.UninstallAsync();
    }

    public async Task ManageSiteMapAsync(AdminMenuItem rootNode)
    {
        if (!_resourcesEnsured)
            await EnsureLocaleResourcesAsync();

        var workingLanguage = await _workContext.GetWorkingLanguageAsync();
        var isPersian = workingLanguage?.LanguageCulture?.StartsWith("fa", StringComparison.OrdinalIgnoreCase) ?? true;
        var langId = workingLanguage?.Id ?? 0;

        var title = await _localizationService.GetResourceAsync("Plugins.Misc.AmazingDiscounts.Manage", langId, returnEmptyIfNotFound: true);
        if (string.IsNullOrWhiteSpace(title) || title.Equals("Plugins.Misc.AmazingDiscounts.Manage", StringComparison.OrdinalIgnoreCase))
        {
            title = isPersian ? "تخفیف‌های شگفت‌انگیز" : "Amazing Discounts";
        }

        var menu = rootNode.GetItemBySystemName("Catalog");
        if (menu != null)
        {
            menu.ChildNodes.Add(new AdminMenuItem
            {
                SystemName = "Misc.AmazingDiscounts",
                Title = title,
                Url = "/Admin/AmazingDiscounts/List",
                IconClass = "far fa-dot-circle",
                Visible = true
            });
        }
    }

    public bool HideInWidgetList => false;
}
