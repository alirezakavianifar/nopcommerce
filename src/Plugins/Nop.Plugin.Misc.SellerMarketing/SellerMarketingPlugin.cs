using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.SellerMarketing;

public class SellerMarketingPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, IWidgetPlugin
{
    #region Fields

    private readonly IWebHelper _webHelper;
    private readonly ILocalizationService _localizationService;
    private readonly ILanguageService _languageService;
    private readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public SellerMarketingPlugin(
        IWebHelper webHelper,
        ILocalizationService localizationService,
        ILanguageService languageService,
        ISettingService settingService)
    {
        _webHelper = webHelper;
        _localizationService = localizationService;
        _languageService = languageService;
        _settingService = settingService;
    }

    #endregion

    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/SellerMarketing/List";
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            PublicWidgetZones.AccountNavigationAfter
        });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(Components.SellerDashboardNavigationViewComponent);
    }

    public bool HideInWidgetList => false;

    public override async Task InstallAsync()
    {
        // Auto-activate widget
        var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>();
        if (!widgetSettings.ActiveWidgetSystemNames.Contains("Misc.SellerMarketing"))
        {
            widgetSettings.ActiveWidgetSystemNames.Add("Misc.SellerMarketing");
            await _settingService.SaveSettingAsync(widgetSettings);
        }

        // Add localization resources
        var languages = await _languageService.GetAllLanguagesAsync();
        var enLang = languages.FirstOrDefault(l => l.LanguageCulture.StartsWith("en", StringComparison.OrdinalIgnoreCase));
        var faLang = languages.FirstOrDefault(l => l.LanguageCulture.StartsWith("fa", StringComparison.OrdinalIgnoreCase));

        var enResources = new Dictionary<string, string>
        {
            ["Plugins.Misc.SellerMarketing.Dashboard.Title"] = "Seller Catalog Dashboard",
            ["Plugins.Misc.SellerMarketing.Dashboard.NoProducts"] = "No submitted products found.",
            ["Plugins.Misc.SellerMarketing.Dashboard.AddProduct"] = "Add New Catalog Product",
            ["Plugins.Misc.SellerMarketing.Dashboard.ProductName"] = "Product Name",
            ["Plugins.Misc.SellerMarketing.Dashboard.SKU"] = "SKU",
            ["Plugins.Misc.SellerMarketing.Dashboard.Price"] = "Price",
            ["Plugins.Misc.SellerMarketing.Dashboard.Stock"] = "Stock Quantity",
            ["Plugins.Misc.SellerMarketing.Dashboard.Status"] = "Approval Status",
            ["Plugins.Misc.SellerMarketing.Dashboard.AdminComment"] = "Admin Feedback",
            ["Plugins.Misc.SellerMarketing.Dashboard.Actions"] = "Actions",
            ["Plugins.Misc.SellerMarketing.Dashboard.Edit"] = "Edit / Resubmit",
            ["Plugins.Misc.SellerMarketing.Dashboard.Delete"] = "Delete",
            ["Plugins.Misc.SellerMarketing.Dashboard.ShortDescription"] = "Short Description",
            ["Plugins.Misc.SellerMarketing.Dashboard.FullDescription"] = "Full Description",
            ["Plugins.Misc.SellerMarketing.Dashboard.ImageFile"] = "Product Image Upload",
            ["Plugins.Misc.SellerMarketing.Dashboard.ImageValidation"] = "Only standard image files (.jpg, .jpeg, .png, .gif) are allowed.",
            ["Plugins.Misc.SellerMarketing.Dashboard.SizeValidation"] = "The uploaded file must be smaller than 5MB.",
            ["Plugins.Misc.SellerMarketing.Dashboard.Submit"] = "Submit Product",
            ["Plugins.Misc.SellerMarketing.Dashboard.Back"] = "Back to Dashboard",
            ["Plugins.Misc.SellerMarketing.Dashboard.MyAccountTab"] = "My Store Dashboard",
            ["Plugins.Misc.SellerMarketing.Admin.Title"] = "Seller Catalog Approvals",
            ["Plugins.Misc.SellerMarketing.Admin.PendingSubmissions"] = "Pending Seller Submissions",
            ["Plugins.Misc.SellerMarketing.Admin.Approve"] = "Approve & Publish",
            ["Plugins.Misc.SellerMarketing.Admin.Reject"] = "Reject / Request Revision",
            ["Plugins.Misc.SellerMarketing.Admin.CommentPlaceholder"] = "Enter reason for rejection or details of requested changes...",
            ["Plugins.Misc.SellerMarketing.Admin.NoSubmissions"] = "No pending catalog submissions found.",
            ["Plugins.Misc.SellerMarketing.Admin.ProductName"] = "Product Name",
            ["Plugins.Misc.SellerMarketing.Admin.Vendor"] = "Seller (Vendor)",
            ["Plugins.Misc.SellerMarketing.Admin.SKU"] = "SKU",
            ["Plugins.Misc.SellerMarketing.Admin.Price"] = "Price",
            ["Plugins.Misc.SellerMarketing.Admin.Details"] = "Submission Details",
            ["Plugins.Misc.SellerMarketing.Admin.Status"] = "Status"
        };

        var faResources = new Dictionary<string, string>
        {
            ["Plugins.Misc.SellerMarketing.Dashboard.Title"] = "داشبورد کاتالوگ فروشنده",
            ["Plugins.Misc.SellerMarketing.Dashboard.NoProducts"] = "هیچ محصولی در کاتالوگ شما یافت نشد.",
            ["Plugins.Misc.SellerMarketing.Dashboard.AddProduct"] = "افزودن محصول جدید به کاتالوگ",
            ["Plugins.Misc.SellerMarketing.Dashboard.ProductName"] = "نام محصول",
            ["Plugins.Misc.SellerMarketing.Dashboard.SKU"] = "شناسه کالا (SKU)",
            ["Plugins.Misc.SellerMarketing.Dashboard.Price"] = "قیمت",
            ["Plugins.Misc.SellerMarketing.Dashboard.Stock"] = "موجودی انبار",
            ["Plugins.Misc.SellerMarketing.Dashboard.Status"] = "وضعیت تایید",
            ["Plugins.Misc.SellerMarketing.Dashboard.AdminComment"] = "توضیحات و بازخورد مدیریت",
            ["Plugins.Misc.SellerMarketing.Dashboard.Actions"] = "عملیات",
            ["Plugins.Misc.SellerMarketing.Dashboard.Edit"] = "ویرایش و ارسال مجدد",
            ["Plugins.Misc.SellerMarketing.Dashboard.Delete"] = "حذف محصول",
            ["Plugins.Misc.SellerMarketing.Dashboard.ShortDescription"] = "توضیح کوتاه",
            ["Plugins.Misc.SellerMarketing.Dashboard.FullDescription"] = "توضیح کامل",
            ["Plugins.Misc.SellerMarketing.Dashboard.ImageFile"] = "بارگذاری تصویر محصول",
            ["Plugins.Misc.SellerMarketing.Dashboard.ImageValidation"] = "فقط فایل‌های تصویری معتبر (.jpg, .jpeg, .png, .gif) مجاز می‌باشند.",
            ["Plugins.Misc.SellerMarketing.Dashboard.SizeValidation"] = "حجم تصویر بارگذاری شده باید کمتر از ۵ مگابایت باشد.",
            ["Plugins.Misc.SellerMarketing.Dashboard.Submit"] = "ثبت و ارسال کاتالوگ",
            ["Plugins.Misc.SellerMarketing.Dashboard.Back"] = "بازگشت به داشبورد",
            ["Plugins.Misc.SellerMarketing.Dashboard.MyAccountTab"] = "داشبورد فروشگاه من",
            ["Plugins.Misc.SellerMarketing.Admin.Title"] = "تایید کاتالوگ فروشندگان",
            ["Plugins.Misc.SellerMarketing.Admin.PendingSubmissions"] = "کاتالوگ‌های در انتظار تایید",
            ["Plugins.Misc.SellerMarketing.Admin.Approve"] = "تایید و انتشار در سایت",
            ["Plugins.Misc.SellerMarketing.Admin.Reject"] = "رد کاتالوگ / درخواست اصلاح",
            ["Plugins.Misc.SellerMarketing.Admin.CommentPlaceholder"] = "علت رد کاتالوگ یا اصلاحات مورد نیاز را وارد نمایید...",
            ["Plugins.Misc.SellerMarketing.Admin.NoSubmissions"] = "هیچ کاتالوگی در انتظار بررسی وجود ندارد.",
            ["Plugins.Misc.SellerMarketing.Admin.ProductName"] = "نام محصول",
            ["Plugins.Misc.SellerMarketing.Admin.Vendor"] = "فروشنده (Vendor)",
            ["Plugins.Misc.SellerMarketing.Admin.SKU"] = "SKU",
            ["Plugins.Misc.SellerMarketing.Admin.Price"] = "قیمت",
            ["Plugins.Misc.SellerMarketing.Admin.Details"] = "جزئیات کاتالوگ",
            ["Plugins.Misc.SellerMarketing.Admin.Status"] = "وضعیت بررسی"
        };

        await _localizationService.AddOrUpdateLocaleResourceAsync(enResources, enLang?.Id);
        await _localizationService.AddOrUpdateLocaleResourceAsync(faResources, faLang?.Id);

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.SellerMarketing");
        await base.UninstallAsync();
    }

    public async Task ManageSiteMapAsync(AdminMenuItem rootNode)
    {
        var catalogMenu = rootNode.GetItemBySystemName("Catalog");
        if (catalogMenu != null)
        {
            catalogMenu.ChildNodes.Add(new AdminMenuItem
            {
                SystemName = "Misc.SellerMarketing.ReviewList",
                Title = await _localizationService.GetResourceAsync("Plugins.Misc.SellerMarketing.Admin.Title") ?? "Seller Catalog Review",
                Url = "/Admin/SellerMarketing/List",
                IconClass = "far fa-check-square",
                Visible = true
            });
        }
    }

    #endregion
}
