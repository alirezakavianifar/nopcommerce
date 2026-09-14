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
    protected readonly ILanguageService _languageService;
    protected readonly IWorkContext _workContext;

    private static bool _resourcesEnsured = false;

    #endregion

    #region Ctor

    public GroupPurchasePlugin(IWebHelper webHelper,
        ILocalizationService localizationService,
        ILanguageService languageService,
        IWorkContext workContext)
    {
        _webHelper = webHelper;
        _localizationService = localizationService;
        _languageService = languageService;
        _workContext = workContext;
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

    private async Task EnsureLocaleResourcesAsync()
    {
        try
        {
            var languages = await _languageService.GetAllLanguagesAsync();
            var enLang = languages.FirstOrDefault(l => l.LanguageCulture.StartsWith("en", StringComparison.OrdinalIgnoreCase));
            var faLang = languages.FirstOrDefault(l => l.LanguageCulture.StartsWith("fa", StringComparison.OrdinalIgnoreCase));

            var enResources = new Dictionary<string, string>
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
                ["Plugins.Misc.GroupPurchase.RewardRule.Fields.MinMembers"] = "Min Members",
                ["Plugins.Misc.GroupPurchase.Wallets.Title"] = "Customer Wallets",

                ["Plugins.Misc.GroupPurchase.Customer.LotteryTitle"] = "My Lottery Points",
                ["Plugins.Misc.GroupPurchase.Customer.LotterySummary"] = "Lottery Points Summary",
                ["Plugins.Misc.GroupPurchase.Customer.TotalPoints"] = "Total Earned Points:",
                ["Plugins.Misc.GroupPurchase.Customer.LotteryInstruction"] = "You can use these points to enter our regular lotteries and win big prizes! Check back for updates on lottery scheduling.",
                
                ["Plugins.Misc.GroupPurchase.Customer.WalletTitle"] = "My Wallet",
                ["Plugins.Misc.GroupPurchase.Customer.WalletBalances"] = "Wallet Balances",
                ["Plugins.Misc.GroupPurchase.Customer.RegularBalance"] = "Regular Balance:",
                ["Plugins.Misc.GroupPurchase.Customer.GroupRewardBalance"] = "Group Purchase Reward Balance:",

                ["Plugins.Misc.GroupPurchase.Customer.LeaderGroupsTitle"] = "My Leader Groups",
                ["Plugins.Misc.GroupPurchase.Customer.GroupCode"] = "Group Code",
                ["Plugins.Misc.GroupPurchase.Customer.Status"] = "Status",
                ["Plugins.Misc.GroupPurchase.Customer.CreatedOn"] = "Created On",
                ["Plugins.Misc.GroupPurchase.Customer.Members"] = "Members",
                ["Plugins.Misc.GroupPurchase.Customer.DeliveryCity"] = "Delivery City",
                ["Plugins.Misc.GroupPurchase.Customer.NoLeaderGroups"] = "You have not created any group purchases yet.",

                ["Plugins.Misc.GroupPurchase.Customer.SubgroupHistoryTitle"] = "My Subgroup History",
                ["Plugins.Misc.GroupPurchase.Customer.JoinedOn"] = "Joined On",
                ["Plugins.Misc.GroupPurchase.Customer.LeaderEmail"] = "Leader Email",
                ["Plugins.Misc.GroupPurchase.Customer.NoSubgroups"] = "You have not joined any group purchases yet.",

                ["Plugins.Misc.GroupPurchase.Customer.WalletTab"] = "My Wallet",
                ["Plugins.Misc.GroupPurchase.Customer.LeaderGroupsTab"] = "My Leader Groups",
                ["Plugins.Misc.GroupPurchase.Customer.SubgroupHistoryTab"] = "My Subgroup History",
                ["Plugins.Misc.GroupPurchase.Customer.LotteryTab"] = "My Lottery Points",

                ["Plugins.Misc.GroupPurchase.Commission.Manage"] = "Manage Commissions",
                ["Plugins.Misc.GroupPurchase.Commission.AddNew"] = "Add Commission",
                ["Plugins.Misc.GroupPurchase.Commission.Edit"] = "Edit Commission",
                ["Plugins.Misc.GroupPurchase.Commission.BackToList"] = "Back to list",
                ["Plugins.Misc.GroupPurchase.Commission.Fields.Scope"] = "Commission Scope",
                ["Plugins.Misc.GroupPurchase.Commission.Fields.EntityId"] = "Entity ID",
                ["Plugins.Misc.GroupPurchase.Commission.Fields.EntityId.Hint"] = "Enter Product ID, Category ID, Vendor ID, or Manufacturer ID based on selected Scope.",
                ["Plugins.Misc.GroupPurchase.Commission.Fields.EntityName"] = "Name / Description",
                ["Plugins.Misc.GroupPurchase.Commission.Fields.Percentage"] = "Commission Percentage (%)",
                ["Plugins.Misc.GroupPurchase.Commission.ImportExcelNote"] = "Upload Excel (.xlsx) with columns: Scope (Product/Category/Vendor/Manufacturer), EntityId, Percentage, Name.",
                ["Plugins.Misc.GroupPurchase.Commission.SyncForoushGostar"] = "Sync with Foroush Gostar",
                ["Plugins.Misc.GroupPurchase.RewardRule.Fields.MinRewardAmount"] = "Min Reward Amount (Floor)",
                ["Plugins.Misc.GroupPurchase.RewardRule.Fields.MaxRewardAmount"] = "Max Reward Amount (Cap)",
                ["Plugins.Misc.GroupPurchase.CustomerClub.Configure"] = "Customer Club Settings",
                ["Plugins.Misc.GroupPurchase.CustomerClub.ConversionRatios"] = "Dual Conversion Ratios",
                ["Plugins.Misc.GroupPurchase.CustomerClub.Fields.EnableDualConversion"] = "Enable Dual Conversion",
                ["Plugins.Misc.GroupPurchase.CustomerClub.Fields.PointsToLotteryChanceRatio"] = "Points per 1 Lottery Chance",
                ["Plugins.Misc.GroupPurchase.CustomerClub.Fields.PointsToLotteryChanceRatio.Hint"] = "E.g. 10 means 10 points = 1 lottery chance.",
                ["Plugins.Misc.GroupPurchase.CustomerClub.Fields.PointsToCurrencyRatio"] = "Currency Value per 1 Point",
                ["Plugins.Misc.GroupPurchase.CustomerClub.Fields.PointsToCurrencyRatio.Hint"] = "E.g. 1000 means 1 point = 1,000 currency units in wallet.",
                ["Plugins.Misc.GroupPurchase.CustomerClub.Fields.DefaultMinRewardAmount"] = "Default Min Reward Amount",
                ["Plugins.Misc.GroupPurchase.CustomerClub.Fields.DefaultMaxRewardAmount"] = "Default Max Reward Amount"
            };

            var faResources = new Dictionary<string, string>
            {
                ["Plugins.Misc.GroupPurchase.Manage"] = "مدیریت خریدهای گروهی",
                ["Plugins.Misc.GroupPurchase.List.Title"] = "خریدهای گروهی",
                ["Plugins.Misc.GroupPurchase.Fields.UniqueCode"] = "کد منحصر به فرد",
                ["Plugins.Misc.GroupPurchase.Fields.LeaderCustomerId"] = "شناسه مشتری لیدر",
                ["Plugins.Misc.GroupPurchase.Fields.Status"] = "وضعیت",
                ["Plugins.Misc.GroupPurchase.Fields.CreatedOnUtc"] = "تاریخ ایجاد",
                ["Plugins.Misc.GroupPurchase.Fields.DeliveryCity"] = "شهر تحویل",
                ["Plugins.Misc.GroupPurchase.Fields.DeliveryAddress"] = "آدرس تحویل",
                ["Plugins.Misc.GroupPurchase.SectionTitle"] = "خرید گروهی",
                ["Plugins.Misc.GroupPurchase.SectionDescription"] = "یک خرید گروهی ایجاد کنید، با دوستان خود به اشتراک بگذارید و پاداش دریافت کنید!",
                ["Plugins.Misc.GroupPurchase.Button.Convert"] = "شروع خرید گروهی",
                ["Plugins.Misc.GroupPurchase.Button.Join"] = "پیوستن به گروه",
                ["Plugins.Misc.GroupPurchase.RewardRule.Manage"] = "مدیریت قوانین پاداش",
                ["Plugins.Misc.GroupPurchase.RewardRule.AddNew"] = "افزودن قانون پاداش جدید",
                ["Plugins.Misc.GroupPurchase.RewardRule.Edit"] = "ویرایش قانون پاداش",
                ["Plugins.Misc.GroupPurchase.RewardRule.BackToList"] = "بازگشت به لیست",
                ["Plugins.Misc.GroupPurchase.RewardRule.Fields.TargetRole"] = "نقش هدف",
                ["Plugins.Misc.GroupPurchase.RewardRule.Fields.RewardType"] = "نوع پاداش",
                ["Plugins.Misc.GroupPurchase.RewardRule.Fields.CalculationType"] = "نوع محاسبه",
                ["Plugins.Misc.GroupPurchase.RewardRule.Fields.Value"] = "مقدار",
                ["Plugins.Misc.GroupPurchase.RewardRule.Fields.CategoryId"] = "شناسه دسته‌بندی (۰ برای همه)",
                ["Plugins.Misc.GroupPurchase.RewardRule.Fields.MinCartAmount"] = "حداقل مبلغ سبد خرید",
                ["Plugins.Misc.GroupPurchase.RewardRule.Fields.MinMembers"] = "حداقل تعداد اعضا",
                ["Plugins.Misc.GroupPurchase.Wallets.Title"] = "کیف پول مشتریان",

                ["Plugins.Misc.GroupPurchase.Customer.LotteryTitle"] = "امتیازات قرعه‌کشی من",
                ["Plugins.Misc.GroupPurchase.Customer.LotterySummary"] = "خلاصه امتیازات قرعه‌کشی",
                ["Plugins.Misc.GroupPurchase.Customer.TotalPoints"] = "مجموع امتیازات کسب‌شده:",
                ["Plugins.Misc.GroupPurchase.Customer.LotteryInstruction"] = "شما می‌توانید از این امتیازات برای شرکت در قرعه‌کشی‌های دوره‌ای و برنده شدن جوایز ویژه استفاده کنید. جهت آگاهی از زمان‌بندی قرعه‌کشی‌ها مجدداً سر بزنید.",

                ["Plugins.Misc.GroupPurchase.Customer.WalletTitle"] = "کیف پول من",
                ["Plugins.Misc.GroupPurchase.Customer.WalletBalances"] = "موجودی‌های کیف پول",
                ["Plugins.Misc.GroupPurchase.Customer.RegularBalance"] = "موجودی عادی:",
                ["Plugins.Misc.GroupPurchase.Customer.GroupRewardBalance"] = "موجودی پاداش خرید گروهی:",

                ["Plugins.Misc.GroupPurchase.Customer.LeaderGroupsTitle"] = "گروه‌های لیدری من",
                ["Plugins.Misc.GroupPurchase.Customer.GroupCode"] = "کد گروه",
                ["Plugins.Misc.GroupPurchase.Customer.Status"] = "وضعیت",
                ["Plugins.Misc.GroupPurchase.Customer.CreatedOn"] = "تاریخ ایجاد",
                ["Plugins.Misc.GroupPurchase.Customer.Members"] = "تعداد اعضا",
                ["Plugins.Misc.GroupPurchase.Customer.DeliveryCity"] = "شهر تحویل",
                ["Plugins.Misc.GroupPurchase.Customer.NoLeaderGroups"] = "شما هنوز هیچ گروه خریدی ایجاد نکرده‌اید.",

                ["Plugins.Misc.GroupPurchase.Customer.SubgroupHistoryTitle"] = "تاریخچه زیرمجموعه‌های من",
                ["Plugins.Misc.GroupPurchase.Customer.JoinedOn"] = "تاریخ عضویت",
                ["Plugins.Misc.GroupPurchase.Customer.LeaderEmail"] = "ایمیل لیدر",
                ["Plugins.Misc.GroupPurchase.Customer.NoSubgroups"] = "شما هنوز در هیچ گروه خریدی عضو نشده‌اید.",

                ["Plugins.Misc.GroupPurchase.Customer.WalletTab"] = "کیف پول من",
                ["Plugins.Misc.GroupPurchase.Customer.LeaderGroupsTab"] = "گروه‌های لیدری من",
                ["Plugins.Misc.GroupPurchase.Customer.SubgroupHistoryTab"] = "تاریخچه زیرمجموعه‌های من",
                ["Plugins.Misc.GroupPurchase.Customer.LotteryTab"] = "امتیازات قرعه‌کشی من",

                ["Plugins.Misc.GroupPurchase.Commission.Manage"] = "مدیریت درصدهای حق‌العمل و سود",
                ["Plugins.Misc.GroupPurchase.Commission.AddNew"] = "افزودن درصد حق‌العمل",
                ["Plugins.Misc.GroupPurchase.Commission.Edit"] = "ویرایش درصد حق‌العمل",
                ["Plugins.Misc.GroupPurchase.Commission.BackToList"] = "بازگشت به لیست",
                ["Plugins.Misc.GroupPurchase.Commission.Fields.Scope"] = "سطح حق‌العمل (کالا/دسته/فروشنده/برند)",
                ["Plugins.Misc.GroupPurchase.Commission.Fields.EntityId"] = "شناسه موجودیت",
                ["Plugins.Misc.GroupPurchase.Commission.Fields.EntityId.Hint"] = "شناسه کالا، دسته‌بندی، فروشنده یا برند را متناسب با سطح انتخاب‌شده وارد نمایید.",
                ["Plugins.Misc.GroupPurchase.Commission.Fields.EntityName"] = "نام / یادداشت",
                ["Plugins.Misc.GroupPurchase.Commission.Fields.Percentage"] = "درصد حق‌العمل (کارمزد)",
                ["Plugins.Misc.GroupPurchase.Commission.ImportExcelNote"] = "فایل اکسل (.xlsx) با ستون‌های Scope (Product, Category, Vendor, Manufacturer)، EntityId، Percentage و Name را بارگذاری نمایید.",
                ["Plugins.Misc.GroupPurchase.Commission.SyncForoushGostar"] = "سینک با فروش‌گستر",
                ["Plugins.Misc.GroupPurchase.RewardRule.Fields.MinRewardAmount"] = "حداقل مبلغ پاداش (کف پاداش)",
                ["Plugins.Misc.GroupPurchase.RewardRule.Fields.MaxRewardAmount"] = "حداکثر مبلغ پاداش (سقف پاداش)",
                ["Plugins.Misc.GroupPurchase.CustomerClub.Configure"] = "تنظیمات باشگاه مشتریان",
                ["Plugins.Misc.GroupPurchase.CustomerClub.ConversionRatios"] = "ضرایب تبدیل دوگانه امتیازات",
                ["Plugins.Misc.GroupPurchase.CustomerClub.Fields.EnableDualConversion"] = "فعال‌سازی تبدیل دوگانه امتیازات",
                ["Plugins.Misc.GroupPurchase.CustomerClub.Fields.PointsToLotteryChanceRatio"] = "امتیاز مورد نیاز به ازای ۱ شانس قرعه‌کشی",
                ["Plugins.Misc.GroupPurchase.CustomerClub.Fields.PointsToLotteryChanceRatio.Hint"] = "مثلاً عدد ۱۰ به این معنی است که هر ۱۰ امتیاز تبدیل به ۱ شانس قرعه‌کشی می‌شود.",
                ["Plugins.Misc.GroupPurchase.CustomerClub.Fields.PointsToCurrencyRatio"] = "ارزش ریالی/تومانی به ازای هر ۱ امتیاز",
                ["Plugins.Misc.GroupPurchase.CustomerClub.Fields.PointsToCurrencyRatio.Hint"] = "مثلاً عدد ۱۰۰۰ به این معنی است که هر ۱ امتیاز معادل ۱۰۰۰ تومان اعتبار کیف پول خواهد بود.",
                ["Plugins.Misc.GroupPurchase.CustomerClub.Fields.DefaultMinRewardAmount"] = "حداقل مبلغ پیش‌فرض پاداش",
                ["Plugins.Misc.GroupPurchase.CustomerClub.Fields.DefaultMaxRewardAmount"] = "حداکثر مبلغ پیش‌فرض پاداش"
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

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        await EnsureLocaleResourcesAsync();
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
        if (!_resourcesEnsured)
            await EnsureLocaleResourcesAsync();

        var workingLanguage = await _workContext.GetWorkingLanguageAsync();
        var isPersian = workingLanguage?.LanguageCulture?.StartsWith("fa", StringComparison.OrdinalIgnoreCase) ?? true;
        var langId = workingLanguage?.Id ?? 0;

        var manageTitle = await _localizationService.GetResourceAsync("Plugins.Misc.GroupPurchase.Manage", langId, returnEmptyIfNotFound: true);
        if (string.IsNullOrWhiteSpace(manageTitle) || manageTitle.Equals("Plugins.Misc.GroupPurchase.Manage", StringComparison.OrdinalIgnoreCase) || (isPersian && manageTitle == "Manage Group Purchases"))
        {
            manageTitle = isPersian ? "مدیریت خریدهای گروهی" : "Manage Group Purchases";
        }

        var rewardRuleTitle = await _localizationService.GetResourceAsync("Plugins.Misc.GroupPurchase.RewardRule.Manage", langId, returnEmptyIfNotFound: true);
        if (string.IsNullOrWhiteSpace(rewardRuleTitle) || rewardRuleTitle.Equals("Plugins.Misc.GroupPurchase.RewardRule.Manage", StringComparison.OrdinalIgnoreCase) || (isPersian && rewardRuleTitle == "Manage Reward Rules"))
        {
            rewardRuleTitle = isPersian ? "مدیریت قوانین پاداش" : "Manage Reward Rules";
        }

        var walletTitle = await _localizationService.GetResourceAsync("Plugins.Misc.GroupPurchase.Wallets.Title", langId, returnEmptyIfNotFound: true);
        if (string.IsNullOrWhiteSpace(walletTitle) || walletTitle.Equals("Plugins.Misc.GroupPurchase.Wallets.Title", StringComparison.OrdinalIgnoreCase) || (isPersian && walletTitle == "Customer Wallets"))
        {
            walletTitle = isPersian ? "کیف پول مشتریان" : "Customer Wallets";
        }

        var menu = rootNode.GetItemBySystemName("Promotions");
        if (menu != null)
        {
            var pluginNode = new AdminMenuItem
            {
                SystemName = "Misc.GroupPurchase",
                Title = manageTitle,
                Url = "/Admin/GroupPurchase/List",
                IconClass = "far fa-dot-circle",
                Visible = true
            };
            menu.ChildNodes.Add(pluginNode);

            var rewardRuleNode = new AdminMenuItem
            {
                SystemName = "Misc.GroupPurchase.RewardRules",
                Title = rewardRuleTitle,
                Url = "/Admin/RewardRule/List",
                IconClass = "far fa-circle",
                Visible = true
            };
            pluginNode.ChildNodes.Add(rewardRuleNode);

            var walletNode = new AdminMenuItem
            {
                SystemName = "Misc.GroupPurchase.Wallets",
                Title = walletTitle,
                Url = "/Admin/CustomerWallet/List",
                IconClass = "fas fa-wallet",
                Visible = true
            };
            pluginNode.ChildNodes.Add(walletNode);

            var commissionNode = new AdminMenuItem
            {
                SystemName = "Misc.GroupPurchase.Commissions",
                Title = isPersian ? "درصدهای حق‌العمل و سود" : "Commissions & Gross Profit",
                Url = "/Admin/CommissionAdmin/List",
                IconClass = "fas fa-percentage",
                Visible = true
            };
            pluginNode.ChildNodes.Add(commissionNode);

            var clubNode = new AdminMenuItem
            {
                SystemName = "Misc.GroupPurchase.CustomerClub",
                Title = isPersian ? "تنظیمات باشگاه مشتریان" : "Customer Club Settings",
                Url = "/Admin/CustomerClubAdmin/Configure",
                IconClass = "fas fa-gift",
                Visible = true
            };
            pluginNode.ChildNodes.Add(clubNode);
        }
    }

    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the administration area
    /// </summary>
    public bool HideInWidgetList => false;

    #endregion
}
