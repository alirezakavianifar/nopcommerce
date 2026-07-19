using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using Nop.Plugin.Misc.ArtificialIntelligence.Domain;

namespace Nop.Plugin.Misc.ArtificialIntelligence;

public class ArtificialIntelligencePlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, IWidgetPlugin
{
    private readonly IWebHelper _webHelper;
    private readonly ILocalizationService _localizationService;
    private readonly ILanguageService _languageService;
    private readonly ISettingService _settingService;

    public ArtificialIntelligencePlugin(
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

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/ArtificialIntelligence/Configure";
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            PublicWidgetZones.SearchBox,
            PublicWidgetZones.BodyEndHtmlTagBefore
        });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == PublicWidgetZones.SearchBox)
            return typeof(Components.AiSearchWidgetViewComponent);

        return typeof(Components.AiChatbotWidgetViewComponent);
    }

    public override async Task InstallAsync()
    {
        // 1. Save default settings
        var settings = new AiSettings();
        await _settingService.SaveSettingAsync(settings);

        // Auto-activate widget
        var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>();
        if (!widgetSettings.ActiveWidgetSystemNames.Contains("Misc.ArtificialIntelligence"))
        {
            widgetSettings.ActiveWidgetSystemNames.Add("Misc.ArtificialIntelligence");
            await _settingService.SaveSettingAsync(widgetSettings);
        }

        // 2. Add localization resources for English and Persian
        var languages = await _languageService.GetAllLanguagesAsync();
        var enLang = languages.FirstOrDefault(l => l.LanguageCulture.StartsWith("en", StringComparison.OrdinalIgnoreCase));
        var faLang = languages.FirstOrDefault(l => l.LanguageCulture.StartsWith("fa", StringComparison.OrdinalIgnoreCase));

        var enResources = new Dictionary<string, string>
        {
            ["Plugins.Misc.ArtificialIntelligence.Configure"] = "Configure AI Settings",
            ["Plugins.Misc.ArtificialIntelligence.SandboxMode"] = "Sandbox/Mock Mode",
            ["Plugins.Misc.ArtificialIntelligence.ApiKey"] = "AvalAI API Key",
            ["Plugins.Misc.ArtificialIntelligence.BaseUrl"] = "Base URL",
            ["Plugins.Misc.ArtificialIntelligence.ChatbotModel"] = "Chatbot Model",
            ["Plugins.Misc.ArtificialIntelligence.VisionModel"] = "Vision/Image Model",
            ["Plugins.Misc.ArtificialIntelligence.EmbeddingModel"] = "Embedding Model",
            ["Plugins.Misc.ArtificialIntelligence.DuplicateSimilarityThreshold"] = "Duplicate Similarity Threshold (0.0 - 1.0)",
            ["Plugins.Misc.ArtificialIntelligence.DuplicateQueue.Title"] = "Duplicate Products Review Queue",
            ["Plugins.Misc.ArtificialIntelligence.DuplicateQueue.Explanation"] = "Vendor Explanation",
            ["Plugins.Misc.ArtificialIntelligence.DuplicateQueue.Status"] = "Status",
            ["Plugins.Misc.ArtificialIntelligence.DuplicateQueue.Approve"] = "Approve As New",
            ["Plugins.Misc.ArtificialIntelligence.DuplicateQueue.Reject"] = "Reject/Block",
            ["Plugins.Misc.ArtificialIntelligence.DuplicateQueue.Pending"] = "Pending Review",
            ["Plugins.Misc.ArtificialIntelligence.Search.VoiceSearch"] = "Voice Search",
            ["Plugins.Misc.ArtificialIntelligence.Search.VisualSearch"] = "Visual Search",
            ["Plugins.Misc.ArtificialIntelligence.Search.MicrophoneAccessBlocked"] = "Microphone access is blocked or unsupported.",
            ["Plugins.Misc.ArtificialIntelligence.Search.Listening"] = "Listening...",
            ["Plugins.Misc.ArtificialIntelligence.Search.VoiceError"] = "An error occurred during voice transcription.",
            ["Plugins.Misc.ArtificialIntelligence.Search.VisualError"] = "An error occurred during image search.",
            ["Plugins.Misc.ArtificialIntelligence.Chatbot.Title"] = "AI Support Assistant",
            ["Plugins.Misc.ArtificialIntelligence.Chatbot.Label"] = "AI Chatbot",
            ["Plugins.Misc.ArtificialIntelligence.Chatbot.Placeholder"] = "Ask a question...",
            ["Plugins.Misc.ArtificialIntelligence.Chatbot.HandoffMessage"] = "I will route your request to our support team. Please enter your name, email, and description below:",
            ["Plugins.Misc.ArtificialIntelligence.Chatbot.HandoffButton"] = "Submit Handoff Request",
            ["Plugins.Misc.ArtificialIntelligence.Chatbot.HandoffSuccess"] = "Your request has been submitted. Our support team will contact you via email.",
            ["Plugins.Misc.ArtificialIntelligence.CreditThreshold"] = "Low Credit Warning Threshold (Tomans)",
            ["Plugins.Misc.ArtificialIntelligence.CreditWarning"] = "AvalAI credit is low. Remaining credit is {0} Tomans, which is below the threshold of {1} Tomans.",
            ["Plugins.Misc.ArtificialIntelligence.CreditFetchError"] = "Could not retrieve AvalAI credit information. Please verify your API Key and connection.",
            ["Plugins.Misc.ArtificialIntelligence.CurrentCredit"] = "Current Remaining Credit",
            ["Plugins.Misc.ArtificialIntelligence.SandboxDescription"] = "Enable Sandbox (No external API calls, tests will use simulated responses)",
            ["Plugins.Misc.ArtificialIntelligence.ApiKeyPlaceholder"] = "Enter AvalAI API Key (Bearer Token)",
            ["Plugins.Misc.ArtificialIntelligence.CreditThresholdDescription"] = "A system warning will be triggered when the remaining credit reaches or drops below this threshold.",
            ["Plugins.Misc.ArtificialIntelligence.ModelSelectionHeader"] = "Model Selection & Parameters",
            ["Plugins.Misc.ArtificialIntelligence.DuplicateSimilarityThresholdDescription"] = "Cosine similarity score above which products are flagged as duplicates. Default is 0.85.",
            ["Plugins.Misc.ArtificialIntelligence.ChatbotModelTooltip"] = "Select the model used for chatbot operations. Input/output costs are shown below.",
            ["Plugins.Misc.ArtificialIntelligence.VisionModelTooltip"] = "Select the model used for image and vision analysis tasks.",
            ["Plugins.Misc.ArtificialIntelligence.EmbeddingModelTooltip"] = "Select the vector embedding model used for product similarity mapping.",
            ["Plugins.Misc.ArtificialIntelligence.Provider"] = "Provider",
            ["Plugins.Misc.ArtificialIntelligence.InputCost"] = "Input Cost",
            ["Plugins.Misc.ArtificialIntelligence.OutputCost"] = "Output Cost",
            ["Plugins.Misc.ArtificialIntelligence.SupportsVision"] = "Supports Vision",
            ["Plugins.Misc.ArtificialIntelligence.Yes"] = "Yes",
            ["Plugins.Misc.ArtificialIntelligence.No"] = "No",
            ["Plugins.Misc.ArtificialIntelligence.UnitsPer1kTokens"] = "units per 1K tokens",
            ["Plugins.Misc.ArtificialIntelligence.ModelDetailsNotAvailable"] = "Configured model. Prices and details not available."
        };

        var faResources = new Dictionary<string, string>
        {
            ["Plugins.Misc.ArtificialIntelligence.Configure"] = "تنظیمات هوش مصنوعی",
            ["Plugins.Misc.ArtificialIntelligence.SandboxMode"] = "حالت تست / شبیه‌ساز",
            ["Plugins.Misc.ArtificialIntelligence.ApiKey"] = "کلید API پلتفرم AvalAI",
            ["Plugins.Misc.ArtificialIntelligence.BaseUrl"] = "آدرس پایه API",
            ["Plugins.Misc.ArtificialIntelligence.ChatbotModel"] = "مدل چت‌بات",
            ["Plugins.Misc.ArtificialIntelligence.VisionModel"] = "مدل تشخیص تصویر",
            ["Plugins.Misc.ArtificialIntelligence.EmbeddingModel"] = "مدل تولید وکتور (Embedding)",
            ["Plugins.Misc.ArtificialIntelligence.DuplicateSimilarityThreshold"] = "حد آستانه شباهت محصول تکراری (بین 0.0 و 1.0)",
            ["Plugins.Misc.ArtificialIntelligence.DuplicateQueue.Title"] = "صف بررسی محصولات تکراری",
            ["Plugins.Misc.ArtificialIntelligence.DuplicateQueue.Explanation"] = "توضیحات فروشنده",
            ["Plugins.Misc.ArtificialIntelligence.DuplicateQueue.Status"] = "وضعیت بررسی",
            ["Plugins.Misc.ArtificialIntelligence.DuplicateQueue.Approve"] = "تایید به عنوان محصول جدید",
            ["Plugins.Misc.ArtificialIntelligence.DuplicateQueue.Reject"] = "رد و مسدود کردن",
            ["Plugins.Misc.ArtificialIntelligence.DuplicateQueue.Pending"] = "در انتظار بررسی",
            ["Plugins.Misc.ArtificialIntelligence.Search.VoiceSearch"] = "جستجوی صوتی",
            ["Plugins.Misc.ArtificialIntelligence.Search.VisualSearch"] = "جستجوی تصویری",
            ["Plugins.Misc.ArtificialIntelligence.Search.MicrophoneAccessBlocked"] = "دسترسی به میکروفون مسدود شده یا پشتیبانی نمی‌شود.",
            ["Plugins.Misc.ArtificialIntelligence.Search.Listening"] = "در حال شنیدن...",
            ["Plugins.Misc.ArtificialIntelligence.Search.VoiceError"] = "خطایی در رونویسی صوتی رخ داد.",
            ["Plugins.Misc.ArtificialIntelligence.Search.VisualError"] = "خطایی در جستجوی تصویری رخ داد.",
            ["Plugins.Misc.ArtificialIntelligence.Chatbot.Title"] = "پشتیبان هوشمند هوش مصنوعی",
            ["Plugins.Misc.ArtificialIntelligence.Chatbot.Label"] = "پشتیبان هوشمند",
            ["Plugins.Misc.ArtificialIntelligence.Chatbot.Placeholder"] = "سوال خود را مطرح کنید...",
            ["Plugins.Misc.ArtificialIntelligence.Chatbot.HandoffMessage"] = "من درخواست شما را به تیم پشتیبانی ارجاع می‌دهم. لطفا نام، ایمیل و شرح درخواست خود را در زیر وارد کنید:",
            ["Plugins.Misc.ArtificialIntelligence.Chatbot.HandoffButton"] = "ارسال درخواست ارجاع",
            ["Plugins.Misc.ArtificialIntelligence.Chatbot.HandoffSuccess"] = "درخواست شما ارسال شد. تیم پشتیبانی ما از طریق ایمیل با شما تماس خواهد گرفت.",
            ["Plugins.Misc.ArtificialIntelligence.CreditThreshold"] = "حد آستانه هشدار اعتبار کم (تومان)",
            ["Plugins.Misc.ArtificialIntelligence.CreditWarning"] = "اعتبار پلتفرم AvalAI رو به اتمام است. اعتبار باقی‌مانده {0} تومان است که کمتر از حد آستانه {1} تومان می‌باشد.",
            ["Plugins.Misc.ArtificialIntelligence.CreditFetchError"] = "امکان دریافت اطلاعات اعتبار پلتفرم AvalAI وجود ندارد. لطفا کلید API و اتصال خود را بررسی کنید.",
            ["Plugins.Misc.ArtificialIntelligence.CurrentCredit"] = "اعتبار باقی‌مانده فعلی",
            ["Plugins.Misc.ArtificialIntelligence.SandboxDescription"] = "فعال‌سازی حالت تست / شبیه‌ساز (بدون فراخوانی API خارجی، تست‌ها از پاسخ‌های شبیه‌سازی شده استفاده خواهند کرد)",
            ["Plugins.Misc.ArtificialIntelligence.ApiKeyPlaceholder"] = "کلید API پلتفرم AvalAI (توکن Bearer) را وارد کنید",
            ["Plugins.Misc.ArtificialIntelligence.CreditThresholdDescription"] = "هنگامی که اعتبار باقی‌مانده به این حد آستانه برسد یا کمتر از آن شود، یک هشدار سیستمی صادر خواهد شد.",
            ["Plugins.Misc.ArtificialIntelligence.ModelSelectionHeader"] = "انتخاب مدل‌ها و پارامترها",
            ["Plugins.Misc.ArtificialIntelligence.DuplicateSimilarityThresholdDescription"] = "حد آستانه شباهت کسینوسی که بالاتر از آن محصولات به عنوان تکراری علامت‌گذاری می‌شوند. پیش‌فرض 0.85 است.",
            ["Plugins.Misc.ArtificialIntelligence.ChatbotModelTooltip"] = "مدل مورد استفاده برای عملیات چت‌بات را انتخاب کنید. هزینه‌های ورودی/خروجی در زیر نشان داده شده است.",
            ["Plugins.Misc.ArtificialIntelligence.VisionModelTooltip"] = "مدل مورد استفاده برای وظایف تجزیه و تحلیل تصویر و بینایی ماشین را انتخاب کنید.",
            ["Plugins.Misc.ArtificialIntelligence.EmbeddingModelTooltip"] = "مدل تعبیه‌ساز وکتور (Embedding) مورد استفاده برای نگاشت شباهت محصولات را انتخاب کنید.",
            ["Plugins.Misc.ArtificialIntelligence.Provider"] = "ارائه‌دهنده",
            ["Plugins.Misc.ArtificialIntelligence.InputCost"] = "هزینه ورودی",
            ["Plugins.Misc.ArtificialIntelligence.OutputCost"] = "هزینه خروجی",
            ["Plugins.Misc.ArtificialIntelligence.SupportsVision"] = "پشتیبانی از پردازش تصویر",
            ["Plugins.Misc.ArtificialIntelligence.Yes"] = "بله",
            ["Plugins.Misc.ArtificialIntelligence.No"] = "خیر",
            ["Plugins.Misc.ArtificialIntelligence.UnitsPer1kTokens"] = "واحد به ازای هر ۱۰۰۰ توکن",
            ["Plugins.Misc.ArtificialIntelligence.ModelDetailsNotAvailable"] = "مدل پیکربندی شده است. قیمت‌ها و جزئیات در دسترس نیستند."
        };

        await _localizationService.AddOrUpdateLocaleResourceAsync(enResources, enLang?.Id);
        await _localizationService.AddOrUpdateLocaleResourceAsync(faResources, faLang?.Id);

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await _settingService.DeleteSettingAsync<AiSettings>();
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.ArtificialIntelligence");
        await base.UninstallAsync();
    }

    public async Task ManageSiteMapAsync(AdminMenuItem rootNode)
    {
        var promotionsMenu = rootNode.GetItemBySystemName("Promotions");
        if (promotionsMenu != null)
        {
            var aiNode = new AdminMenuItem
            {
                SystemName = "Misc.ArtificialIntelligence",
                Title = await _localizationService.GetResourceAsync("Plugins.Misc.ArtificialIntelligence.Title") ?? "AI Hub (AvalAI)",
                Url = string.Empty,
                IconClass = "fas fa-brain",
                Visible = true
            };
            promotionsMenu.ChildNodes.Add(aiNode);

            var configNode = new AdminMenuItem
            {
                SystemName = "Misc.ArtificialIntelligence.Configure",
                Title = await _localizationService.GetResourceAsync("Plugins.Misc.ArtificialIntelligence.Configure") ?? "Configure AI Settings",
                Url = "/Admin/ArtificialIntelligence/Configure",
                IconClass = "fas fa-cog",
                Visible = true
            };
            aiNode.ChildNodes.Add(configNode);

            var duplicateQueueNode = new AdminMenuItem
            {
                SystemName = "Misc.ArtificialIntelligence.DuplicateQueue",
                Title = await _localizationService.GetResourceAsync("Plugins.Misc.ArtificialIntelligence.DuplicateQueue.Title") ?? "AI Duplicate Queue",
                Url = "/Admin/AiDuplicateProduct/List",
                IconClass = "far fa-copy",
                Visible = true
            };
            aiNode.ChildNodes.Add(duplicateQueueNode);
        }
    }

    public bool HideInWidgetList => false;
}
