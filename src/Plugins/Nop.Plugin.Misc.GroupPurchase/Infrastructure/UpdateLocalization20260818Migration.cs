using System.Collections.Generic;
using System.Linq;
using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.GroupPurchase.Infrastructure;

[NopMigration("2026/08/18 10:00:00", "GroupPurchase localization update for EN and FA menu and features", MigrationProcessType.Update)]
public class UpdateLocalization20260818Migration : MigrationBase
{
    public override void Down()
    {
    }

    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();

        var languages = languageService.GetAllLanguages(true);

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
            ["Plugins.Misc.GroupPurchase.Customer.LotteryTab"] = "My Lottery Points"
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
            ["Plugins.Misc.GroupPurchase.Customer.LotteryTab"] = "امتیازات قرعه‌کشی من"
        };

        // Add to default (no language ID)
        localizationService.AddOrUpdateLocaleResource(faResources);

        // Add to all registered languages
        foreach (var lang in languages)
        {
            if (lang.LanguageCulture.StartsWith("fa", System.StringComparison.OrdinalIgnoreCase))
            {
                localizationService.AddOrUpdateLocaleResource(faResources, lang.Id);
            }
            else
            {
                localizationService.AddOrUpdateLocaleResource(enResources, lang.Id);
            }
        }
    }
}
