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
            ["Plugins.Misc.SellerMarketing.Admin.Status"] = "Status",

            ["Plugins.Misc.SellerMarketing.Backup.Title"] = "Seller Backup & Restore",
            ["Plugins.Misc.SellerMarketing.Backup.AdminSettings"] = "Seller Backup Settings",
            ["Plugins.Misc.SellerMarketing.Backup.RestoreRequestsQueue"] = "Seller Restore Requests",
            ["Plugins.Misc.SellerMarketing.Backup.ViewRequests"] = "View Restore Requests",
            ["Plugins.Misc.SellerMarketing.Backup.EntityPermissions"] = "Entity Backup Permissions (Admin Control)",
            ["Plugins.Misc.SellerMarketing.Backup.AllowProducts"] = "Allow Products",
            ["Plugins.Misc.SellerMarketing.Backup.AllowProductAttributes"] = "Allow Product Attributes",
            ["Plugins.Misc.SellerMarketing.Backup.AllowPictures"] = "Allow Product Pictures",
            ["Plugins.Misc.SellerMarketing.Backup.AllowOrders"] = "Allow Orders (Restricted)",
            ["Plugins.Misc.SellerMarketing.Backup.AllowOrders.RestrictedHint"] = "Restricted: vendors cannot backup order records.",
            ["Plugins.Misc.SellerMarketing.Backup.AllowFinancialData"] = "Allow Financial Reports (Restricted)",
            ["Plugins.Misc.SellerMarketing.Backup.AllowFinancialData.RestrictedHint"] = "Restricted: vendors cannot backup financial records.",
            ["Plugins.Misc.SellerMarketing.Backup.SecurityPolicies"] = "Security & Source Policies",
            ["Plugins.Misc.SellerMarketing.Backup.AllowExternalFileRestore"] = "Allow External File Restore",
            ["Plugins.Misc.SellerMarketing.Backup.AllowExternalFileRestore.Hint"] = "When disabled, restore is strictly restricted to server-stored archives only.",
            ["Plugins.Misc.SellerMarketing.Backup.RequireAdminApproval"] = "Require Admin Approval For Restore",
            ["Plugins.Misc.SellerMarketing.Backup.HmacSecretKey"] = "HMAC Signature Secret Key",
            ["Plugins.Misc.SellerMarketing.Backup.HmacSecretKey.Hint"] = "Server-side secret key used to seal manifests and verify anti-tampering.",
            ["Plugins.Misc.SellerMarketing.Backup.CreateSection"] = "Create Server Backup",
            ["Plugins.Misc.SellerMarketing.Backup.CreateDescription"] = "Take a complete snapshot of your catalog data. It will be stored securely on the internal server with a copy available for download.",
            ["Plugins.Misc.SellerMarketing.Backup.CreateButton"] = "Create Backup Now",
            ["Plugins.Misc.SellerMarketing.Backup.ListTitle"] = "Your Server Backups",
            ["Plugins.Misc.SellerMarketing.Backup.FileName"] = "File Name",
            ["Plugins.Misc.SellerMarketing.Backup.IncludedEntities"] = "Included Data",
            ["Plugins.Misc.SellerMarketing.Backup.FileSize"] = "File Size",
            ["Plugins.Misc.SellerMarketing.Backup.CreatedOn"] = "Created On",
            ["Plugins.Misc.SellerMarketing.Backup.RequestRestore"] = "Request Restore",
            ["Plugins.Misc.SellerMarketing.Backup.ConfirmRestore"] = "Are you sure you want to submit a restore request for this backup? It will be sent to the administrator for approval.",
            ["Plugins.Misc.SellerMarketing.Backup.NoBackups"] = "No server backups found. Click 'Create Backup Now' to generate one.",
            ["Plugins.Misc.SellerMarketing.Backup.ExternalUploadTitle"] = "Upload External Backup Archive",
            ["Plugins.Misc.SellerMarketing.Backup.ExternalUploadNote"] = "Only genuine, untampered zip archives created by this store will pass cryptographic HMAC-SHA256 signature verification.",
            ["Plugins.Misc.SellerMarketing.Backup.UploadAndRestore"] = "Upload & Request Restore",
            ["Plugins.Misc.SellerMarketing.Backup.RestoreRequestsTitle"] = "Your Restore Requests",
            ["Plugins.Misc.SellerMarketing.Backup.Source"] = "Backup Source",
            ["Plugins.Misc.SellerMarketing.Backup.Status"] = "Approval Status",
            ["Plugins.Misc.SellerMarketing.Backup.RequestedOn"] = "Requested On",
            ["Plugins.Misc.SellerMarketing.Backup.AdminComment"] = "Admin Notes / Reason",
            ["Plugins.Misc.SellerMarketing.Backup.ExternalFile"] = "External Upload",
            ["Plugins.Misc.SellerMarketing.Backup.ServerBackup"] = "Server Archive",
            ["Plugins.Misc.SellerMarketing.Backup.Status.Pending"] = "Pending Admin Approval",
            ["Plugins.Misc.SellerMarketing.Backup.Status.Approved"] = "Approved & Restored",
            ["Plugins.Misc.SellerMarketing.Backup.NoRequests"] = "No restore requests found.",
            ["Plugins.Misc.SellerMarketing.Backup.Vendor"] = "Seller (Vendor)"
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
            ["Plugins.Misc.SellerMarketing.Admin.Status"] = "وضعیت بررسی",

            ["Plugins.Misc.SellerMarketing.Backup.Title"] = "پشتیبان‌گیری و بازیابی اطلاعات فروشنده",
            ["Plugins.Misc.SellerMarketing.Backup.AdminSettings"] = "تنظیمات بکاپ و ریستور فروشندگان",
            ["Plugins.Misc.SellerMarketing.Backup.RestoreRequestsQueue"] = "درخواست‌های بازیابی اطلاعات فروشندگان",
            ["Plugins.Misc.SellerMarketing.Backup.ViewRequests"] = "مشاهده درخواست‌های ریستور",
            ["Plugins.Misc.SellerMarketing.Backup.EntityPermissions"] = "دسترسی‌های بکاپ موجودیت‌ها (مدیریت توسط ادمین)",
            ["Plugins.Misc.SellerMarketing.Backup.AllowProducts"] = "اجازه بکاپ محصولات",
            ["Plugins.Misc.SellerMarketing.Backup.AllowProductAttributes"] = "اجازه بکاپ ویژگی‌ها و مشخصات کالا",
            ["Plugins.Misc.SellerMarketing.Backup.AllowPictures"] = "اجازه بکاپ تصاویر محصولات",
            ["Plugins.Misc.SellerMarketing.Backup.AllowOrders"] = "اجازه بکاپ سفارشات (محدود شده)",
            ["Plugins.Misc.SellerMarketing.Backup.AllowOrders.RestrictedHint"] = "محدود شده طبق سیاست سیستم: فروشندگان نباید به بکاپ سفارشات دسترسی داشته باشند.",
            ["Plugins.Misc.SellerMarketing.Backup.AllowFinancialData"] = "اجازه بکاپ اطلاعات مالی و حسابرسی (محدود شده)",
            ["Plugins.Misc.SellerMarketing.Backup.AllowFinancialData.RestrictedHint"] = "محدود شده طبق سیاست سیستم: فروشندگان نباید به سوابق مالی دسترسی داشته باشند.",
            ["Plugins.Misc.SellerMarketing.Backup.SecurityPolicies"] = "سیاست‌های امنیتی و منبع فایل",
            ["Plugins.Misc.SellerMarketing.Backup.AllowExternalFileRestore"] = "امکان بارگذاری فایل ریستور از خارج سیستم",
            ["Plugins.Misc.SellerMarketing.Backup.AllowExternalFileRestore.Hint"] = "در صورت غیرفعال بودن، ریستور منحصراً از فایل‌های آرشیو موجود روی سرور داخلی مجاز خواهد بود.",
            ["Plugins.Misc.SellerMarketing.Backup.RequireAdminApproval"] = "الزام تایید ادمین برای انجام ریستور",
            ["Plugins.Misc.SellerMarketing.Backup.HmacSecretKey"] = "کلید اختصاصی امضای امنیتی (HMAC Secret)",
            ["Plugins.Misc.SellerMarketing.Backup.HmacSecretKey.Hint"] = "کلید محرمانه سرور جهت مهر و موم دیجیتال و راستی‌آزمایی عدم دستکاری فایل‌ها.",
            ["Plugins.Misc.SellerMarketing.Backup.CreateSection"] = "ایجاد نسخه پشتیبان روی سرور",
            ["Plugins.Misc.SellerMarketing.Backup.CreateDescription"] = "یک نسخه کامل از اطلاعات کاتالوگ خود تهیه نمایید. این نسخه به‌صورت امن روی سرور داخلی ذخیره شده و امکان دانلود فایل نیز وجود دارد.",
            ["Plugins.Misc.SellerMarketing.Backup.CreateButton"] = "ایجاد نسخه پشتیبان جدید",
            ["Plugins.Misc.SellerMarketing.Backup.ListTitle"] = "نسخه‌های پشتیبان شما در سرور",
            ["Plugins.Misc.SellerMarketing.Backup.FileName"] = "نام فایل",
            ["Plugins.Misc.SellerMarketing.Backup.IncludedEntities"] = "محتوای بکاپ",
            ["Plugins.Misc.SellerMarketing.Backup.FileSize"] = "حجم فایل",
            ["Plugins.Misc.SellerMarketing.Backup.CreatedOn"] = "تاریخ ایجاد",
            ["Plugins.Misc.SellerMarketing.Backup.RequestRestore"] = "درخواست بازیابی",
            ["Plugins.Misc.SellerMarketing.Backup.ConfirmRestore"] = "آیا برای ارسال درخواست بازیابی این فایل پشتیبان اطمینان دارید؟ این عملیات پس از بررسی و تایید مدیر سایت اجرا خواهد شد.",
            ["Plugins.Misc.SellerMarketing.Backup.NoBackups"] = "هیچ نسخه پشتیبانی روی سرور یافت نشد. برای ایجاد، روی 'ایجاد نسخه پشتیبان جدید' کلیک کنید.",
            ["Plugins.Misc.SellerMarketing.Backup.ExternalUploadTitle"] = "بارگذاری فایل پشتیبان از دستگاه",
            ["Plugins.Misc.SellerMarketing.Backup.ExternalUploadNote"] = "توجه: فقط فایل‌های زیپ معتبر و دست‌نخورده که توسط همین سیستم تولید شده باشند، از اعتبارسنجی امضای دیجیتال HMAC عبور خواهند کرد.",
            ["Plugins.Misc.SellerMarketing.Backup.UploadAndRestore"] = "بارگذاری و درخواست ریستور",
            ["Plugins.Misc.SellerMarketing.Backup.RestoreRequestsTitle"] = "درخواست‌های بازیابی ثبت‌شده شما",
            ["Plugins.Misc.SellerMarketing.Backup.Source"] = "منبع بکاپ",
            ["Plugins.Misc.SellerMarketing.Backup.Status"] = "وضعیت بررسی",
            ["Plugins.Misc.SellerMarketing.Backup.RequestedOn"] = "تاریخ درخواست",
            ["Plugins.Misc.SellerMarketing.Backup.AdminComment"] = "توضیحات مدیریت",
            ["Plugins.Misc.SellerMarketing.Backup.ExternalFile"] = "فایل بارگذاری‌شده",
            ["Plugins.Misc.SellerMarketing.Backup.ServerBackup"] = "آرشیو سرور",
            ["Plugins.Misc.SellerMarketing.Backup.Status.Pending"] = "در انتظار تایید ادمین",
            ["Plugins.Misc.SellerMarketing.Backup.Status.Approved"] = "تایید و بازیابی شد",
            ["Plugins.Misc.SellerMarketing.Backup.NoRequests"] = "هیچ درخواست بازیابی ثبت نشده است.",
            ["Plugins.Misc.SellerMarketing.Backup.Vendor"] = "فروشنده (Vendor)"
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

            catalogMenu.ChildNodes.Add(new AdminMenuItem
            {
                SystemName = "Misc.SellerMarketing.BackupSettings",
                Title = await _localizationService.GetResourceAsync("Plugins.Misc.SellerMarketing.Backup.AdminSettings") ?? "Seller Backup Settings",
                Url = "/Admin/SellerBackupAdmin/Configure",
                IconClass = "fas fa-shield-alt",
                Visible = true
            });

            catalogMenu.ChildNodes.Add(new AdminMenuItem
            {
                SystemName = "Misc.SellerMarketing.RestoreRequests",
                Title = await _localizationService.GetResourceAsync("Plugins.Misc.SellerMarketing.Backup.RestoreRequestsQueue") ?? "Seller Restore Requests",
                Url = "/Admin/SellerBackupAdmin/RestoreRequests",
                IconClass = "fas fa-history",
                Visible = true
            });
        }
    }

    #endregion
}
