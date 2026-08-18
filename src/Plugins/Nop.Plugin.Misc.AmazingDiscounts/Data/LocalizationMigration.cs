using System.Collections.Generic;
using System.Linq;
using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.AmazingDiscounts.Data;

[NopMigration("2026/08/18 10:00:00", "AmazingDiscounts localization update for EN and FA", MigrationProcessType.Update)]
public class LocalizationMigration : MigrationBase
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
