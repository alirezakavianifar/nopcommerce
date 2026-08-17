using Nop.Core;
using Nop.Plugin.Misc.UserNotifications.Components;
using Nop.Plugin.Misc.UserNotifications.Domain;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.UserNotifications;

public class UserNotificationsPlugin : BasePlugin, IMiscPlugin, IWidgetPlugin, IAdminMenuPlugin
{
    protected readonly IWebHelper _webHelper;
    protected readonly ILocalizationService _localizationService;
    protected readonly ILanguageService _languageService;
    protected readonly IScheduleTaskService _scheduleTaskService;
    protected readonly ISettingService _settingService;

    public UserNotificationsPlugin(
        IWebHelper webHelper,
        ILocalizationService localizationService,
        ILanguageService languageService,
        IScheduleTaskService scheduleTaskService,
        ISettingService settingService)
    {
        _webHelper = webHelper;
        _localizationService = localizationService;
        _languageService = languageService;
        _scheduleTaskService = scheduleTaskService;
        _settingService = settingService;
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/UserNotifications/Workflows";
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            PublicWidgetZones.HomepageTop,
            PublicWidgetZones.HeaderBefore,
            PublicWidgetZones.HeaderLinksAfter,
            PublicWidgetZones.BodyStartHtmlTagAfter,
            PublicWidgetZones.AccountNavigationAfter
        });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == PublicWidgetZones.HeaderLinksAfter)
            return typeof(NotificationsInboxViewComponent);

        if (widgetZone == PublicWidgetZones.BodyStartHtmlTagAfter)
            return typeof(PopupModalViewComponent);

        if (widgetZone == PublicWidgetZones.AccountNavigationAfter)
            return typeof(AccountNavigationNotificationsViewComponent);

        return typeof(UserNotificationsViewComponent);
    }

    public override async Task InstallAsync()
    {
        // FarazSMS default settings
        var settings = new FarazSmsSettings
        {
            Enabled = false,
            ApiUrl = "https://ippanel.com/api/select",
            ApiKey = "",
            SenderNumber = "+983000505",
            DefaultPatternCode = ""
        };
        await _settingService.SaveSettingAsync(settings);

        // Schedule Task registration
        var taskType = "Nop.Plugin.Misc.UserNotifications.Tasks.ProcessNotificationWorkflowsTask, Nop.Plugin.Misc.UserNotifications";
        if (await _scheduleTaskService.GetTaskByTypeAsync(taskType) is null)
        {
            await _scheduleTaskService.InsertTaskAsync(new Nop.Core.Domain.ScheduleTasks.ScheduleTask
            {
                Enabled = true,
                StopOnError = false,
                Name = "Process Notification Workflows Queue",
                Type = taskType,
                Seconds = 60
            });
        }

        await InstallLocaleResourcesAsync();
        await base.InstallAsync();
    }

    public async Task InstallLocaleResourcesAsync()
    {
        var languages = await _languageService.GetAllLanguagesAsync();

        var enResources = new Dictionary<string, string>
        {
            ["Plugins.Misc.UserNotifications.Announcements"] = "System Announcements",
            ["Plugins.Misc.UserNotifications.Workflows"] = "Notification Workflows",
            ["Plugins.Misc.UserNotifications.FarazSms"] = "FarazSMS Settings",
            ["Plugins.Misc.UserNotifications.Queue"] = "Delivery Queue Logs",
            ["Plugins.Misc.UserNotifications.Inbox"] = "My Notifications",
            ["Plugins.Misc.UserNotifications.Inbox.Unread"] = "Unread",
            ["Plugins.Misc.UserNotifications.Inbox.Empty"] = "You have no notifications in your account inbox.",
            ["Plugins.Misc.UserNotifications.Inbox.AllCaughtUp"] = "You're all caught up! There are no unread notifications right now.",
            ["Plugins.Misc.UserNotifications.Inbox.New"] = "New",
            ["Plugins.Misc.UserNotifications.Inbox.ViewDetails"] = "View Details",
            ["Plugins.Misc.UserNotifications.Popup.Claim"] = "Claim Special Offer",
            ["Plugins.Misc.UserNotifications.Popup.Dismiss"] = "Dismiss",
            ["Plugins.Misc.UserNotifications.AddAnnouncement"] = "Add a new announcement",
            ["Plugins.Misc.UserNotifications.EditAnnouncement"] = "Edit announcement",
            ["Plugins.Misc.UserNotifications.AddWorkflow"] = "Add new workflow",
            ["Plugins.Misc.UserNotifications.WorkflowName"] = "Workflow Name",
            ["Plugins.Misc.UserNotifications.TriggerAction"] = "Trigger Action",
            ["Plugins.Misc.UserNotifications.IsActive"] = "Is Active",
            ["Plugins.Misc.UserNotifications.CreatedOn"] = "Created On (UTC)",
            ["Plugins.Misc.UserNotifications.Actions"] = "Actions",
            ["Plugins.Misc.UserNotifications.ConfigureSteps"] = "Configure Steps",
            ["Plugins.Misc.UserNotifications.SaveWorkflow"] = "Save Workflow",
            ["Plugins.Misc.UserNotifications.BackToWorkflows"] = "Back to Workflows",
            ["Plugins.Misc.UserNotifications.BackToList"] = "back to list",
            ["Plugins.Misc.UserNotifications.Fields.Title"] = "Title",
            ["Plugins.Misc.UserNotifications.Fields.Body"] = "Body",
            ["Plugins.Misc.UserNotifications.Fields.StartDateUtc"] = "Start Date (UTC)",
            ["Plugins.Misc.UserNotifications.Fields.EndDateUtc"] = "End Date (UTC)",
            ["Plugins.Misc.UserNotifications.Fields.IsPublished"] = "Is published",
            ["Plugins.Misc.UserNotifications.Added"] = "The announcement has been added successfully.",
            ["Plugins.Misc.UserNotifications.Updated"] = "The announcement has been updated successfully.",
            ["Plugins.Misc.UserNotifications.Deleted"] = "The announcement has been deleted successfully.",
            ["Plugins.Misc.UserNotifications.FilterAll"] = "All",
            ["Plugins.Misc.UserNotifications.FilterOrders"] = "Orders",
            ["Plugins.Misc.UserNotifications.FilterOffers"] = "Offers & Deals",
            ["Plugins.Misc.UserNotifications.FilterSystem"] = "System",
            ["Plugins.Misc.UserNotifications.MarkAllAsRead"] = "Mark all as read",
            ["Plugins.Misc.UserNotifications.MarkAsRead"] = "Mark as read",
            ["Plugins.Misc.UserNotifications.ClearRead"] = "Clear read",
            ["Plugins.Misc.UserNotifications.CopyCode"] = "Copy Code",
            ["Plugins.Misc.UserNotifications.PromoCode"] = "PROMO CODE",
            ["Plugins.Misc.UserNotifications.SearchPlaceholder"] = "Search notifications, offers, or codes...",
            ["Plugins.Misc.UserNotifications.ViewAllNotifications"] = "View all in Inbox",
            ["Plugins.Misc.UserNotifications.Preferences"] = "Notification Preferences",
            ["Plugins.Misc.UserNotifications.BackToInbox"] = "Back to Inbox",
            ["Plugins.Misc.UserNotifications.PreferencesSaved"] = "Your notification preferences have been saved successfully.",
            ["Plugins.Misc.UserNotifications.Preferences.ChannelsTitle"] = "Delivery Channels",
            ["Plugins.Misc.UserNotifications.Preferences.ChannelsDesc"] = "Choose how and where you want to receive notifications from us.",
            ["Plugins.Misc.UserNotifications.Preferences.OnSiteToasts"] = "On-Site Toast Popups",
            ["Plugins.Misc.UserNotifications.Preferences.OnSiteToastsDesc"] = "Show non-intrusive floating toasts on screen while browsing.",
            ["Plugins.Misc.UserNotifications.Preferences.Sound"] = "Notification Sound Chime",
            ["Plugins.Misc.UserNotifications.Preferences.SoundDesc"] = "Play a subtle pleasant audio chime when a new notification arrives.",
            ["Plugins.Misc.UserNotifications.Preferences.TestSound"] = "Test audio chime",
            ["Plugins.Misc.UserNotifications.Preferences.Email"] = "Email Notifications",
            ["Plugins.Misc.UserNotifications.Preferences.EmailDesc"] = "Receive order updates, dispatch notes, and special offers in your email.",
            ["Plugins.Misc.UserNotifications.Preferences.Sms"] = "SMS Notifications",
            ["Plugins.Misc.UserNotifications.Preferences.SmsDesc"] = "Get real-time instant SMS for order status and flash deals.",
            ["Plugins.Misc.UserNotifications.Preferences.TopicsTitle"] = "Notification Topics",
            ["Plugins.Misc.UserNotifications.Preferences.TopicsDesc"] = "Select the types of updates you are interested in.",
            ["Plugins.Misc.UserNotifications.Preferences.OrderUpdates"] = "Orders & Deliveries",
            ["Plugins.Misc.UserNotifications.Preferences.OrderUpdatesDesc"] = "Order confirmation, payment receipts, shipment tracking, and delivery alerts.",
            ["Plugins.Misc.UserNotifications.Preferences.Promotions"] = "Promotions & Discounts",
            ["Plugins.Misc.UserNotifications.Preferences.PromotionsDesc"] = "Special vouchers, dynamic coupons, abandoned cart reminders, and sales.",
            ["Plugins.Misc.UserNotifications.Preferences.Announcements"] = "Store Announcements",
            ["Plugins.Misc.UserNotifications.Preferences.AnnouncementsDesc"] = "Holiday schedules, maintenance, new features, and policy updates.",
            ["Plugins.Misc.UserNotifications.StartShopping"] = "Start Shopping",
            ["Plugins.Misc.UserNotifications.Announcement"] = "Announcement"
        };

        var faResources = new Dictionary<string, string>
        {
            ["Plugins.Misc.UserNotifications.Announcements"] = "اطلاعیه‌های سیستم",
            ["Plugins.Misc.UserNotifications.Workflows"] = "فرآیندهای خودکار اعلانات",
            ["Plugins.Misc.UserNotifications.FarazSms"] = "تنظیمات FarazSMS",
            ["Plugins.Misc.UserNotifications.Queue"] = "گزارش ارسال اعلانات",
            ["Plugins.Misc.UserNotifications.Inbox"] = "اعلان‌های من",
            ["Plugins.Misc.UserNotifications.Inbox.Unread"] = "خوانده‌نشده",
            ["Plugins.Misc.UserNotifications.Inbox.Empty"] = "هیچ اعلانی در صندوق ورودی شما وجود ندارد.",
            ["Plugins.Misc.UserNotifications.Inbox.AllCaughtUp"] = "همه پیام‌ها خوانده شده است! هیچ اعلان جدیدی وجود ندارد.",
            ["Plugins.Misc.UserNotifications.Inbox.New"] = "جدید",
            ["Plugins.Misc.UserNotifications.Inbox.ViewDetails"] = "مشاهده جزئیات",
            ["Plugins.Misc.UserNotifications.Popup.Claim"] = "دریافت پیشنهاد ویژه",
            ["Plugins.Misc.UserNotifications.Popup.Dismiss"] = "بستن",
            ["Plugins.Misc.UserNotifications.AddAnnouncement"] = "افزودن اطلاعیه جدید",
            ["Plugins.Misc.UserNotifications.EditAnnouncement"] = "ویرایش اطلاعیه",
            ["Plugins.Misc.UserNotifications.AddWorkflow"] = "افزودن فرآیند جدید",
            ["Plugins.Misc.UserNotifications.WorkflowName"] = "نام فرآیند",
            ["Plugins.Misc.UserNotifications.TriggerAction"] = "اقدام محرک",
            ["Plugins.Misc.UserNotifications.IsActive"] = "وضعیت فعال",
            ["Plugins.Misc.UserNotifications.CreatedOn"] = "تاریخ ایجاد (UTC)",
            ["Plugins.Misc.UserNotifications.Actions"] = "عملیات",
            ["Plugins.Misc.UserNotifications.ConfigureSteps"] = "پیکربندی مراحل",
            ["Plugins.Misc.UserNotifications.SaveWorkflow"] = "ذخیره فرآیند",
            ["Plugins.Misc.UserNotifications.BackToWorkflows"] = "بازگشت به لیست فرآیندها",
            ["Plugins.Misc.UserNotifications.BackToList"] = "بازگشت به لیست",
            ["Plugins.Misc.UserNotifications.Fields.Title"] = "عنوان",
            ["Plugins.Misc.UserNotifications.Fields.Body"] = "متن پیام",
            ["Plugins.Misc.UserNotifications.Fields.StartDateUtc"] = "تاریخ شروع (UTC)",
            ["Plugins.Misc.UserNotifications.Fields.EndDateUtc"] = "تاریخ پایان (UTC)",
            ["Plugins.Misc.UserNotifications.Fields.IsPublished"] = "منتشر شده",
            ["Plugins.Misc.UserNotifications.Added"] = "اطلاعیه با موفقیت افزوده شد.",
            ["Plugins.Misc.UserNotifications.Updated"] = "اطلاعیه با موفقیت بروزرسانی شد.",
            ["Plugins.Misc.UserNotifications.Deleted"] = "اطلاعیه با موفقیت حذف شد.",
            ["Plugins.Misc.UserNotifications.FilterAll"] = "همه",
            ["Plugins.Misc.UserNotifications.FilterOrders"] = "سفارش‌ها",
            ["Plugins.Misc.UserNotifications.FilterOffers"] = "تخفیف‌ها و پیشنهادات",
            ["Plugins.Misc.UserNotifications.FilterSystem"] = "سیستم",
            ["Plugins.Misc.UserNotifications.MarkAllAsRead"] = "علامت‌گذاری همه به عنوان خوانده شده",
            ["Plugins.Misc.UserNotifications.MarkAsRead"] = "خوانده شد",
            ["Plugins.Misc.UserNotifications.ClearRead"] = "پاکسازی خوانده‌شده‌ها",
            ["Plugins.Misc.UserNotifications.CopyCode"] = "کپی کد",
            ["Plugins.Misc.UserNotifications.PromoCode"] = "کد تخفیف",
            ["Plugins.Misc.UserNotifications.SearchPlaceholder"] = "جستجوی اعلان‌ها، تخفیف‌ها یا کدها...",
            ["Plugins.Misc.UserNotifications.ViewAllNotifications"] = "مشاهده همه اعلان‌ها",
            ["Plugins.Misc.UserNotifications.Preferences"] = "تنظیمات دریافت اعلانات",
            ["Plugins.Misc.UserNotifications.BackToInbox"] = "بازگشت به صندوق پیام‌ها",
            ["Plugins.Misc.UserNotifications.PreferencesSaved"] = "تنظیمات اعلانات شما با موفقیت ذخیره شد.",
            ["Plugins.Misc.UserNotifications.Preferences.ChannelsTitle"] = "کانال‌های دریافت",
            ["Plugins.Misc.UserNotifications.Preferences.ChannelsDesc"] = "انتخاب کنید اعلانات از چه طریق به دست شما برسد.",
            ["Plugins.Misc.UserNotifications.Preferences.OnSiteToasts"] = "اعلان‌های شناور در سایت",
            ["Plugins.Misc.UserNotifications.Preferences.OnSiteToastsDesc"] = "نمایش پیام‌های شناور و پیشنهادات هنگام گشت‌وگذار در فروشگاه.",
            ["Plugins.Misc.UserNotifications.Preferences.Sound"] = "صدای زنگ اعلان",
            ["Plugins.Misc.UserNotifications.Preferences.SoundDesc"] = "پخش صدای ملایم هنگام دریافت اعلان جدید.",
            ["Plugins.Misc.UserNotifications.Preferences.TestSound"] = "تست صدای زنگ",
            ["Plugins.Misc.UserNotifications.Preferences.Email"] = "اعلان‌های ایمیلی",
            ["Plugins.Misc.UserNotifications.Preferences.EmailDesc"] = "دریافت وضعیت سفارشات و پیشنهادات ویژه از طریق ایمیل.",
            ["Plugins.Misc.UserNotifications.Preferences.Sms"] = "پیامک‌های اطلاع‌رسانی",
            ["Plugins.Misc.UserNotifications.Preferences.SmsDesc"] = "دریافت پیامک فوری برای رهگیری سفارش و تخفیف‌های لحظه‌ای.",
            ["Plugins.Misc.UserNotifications.Preferences.TopicsTitle"] = "موضوعات اعلانات",
            ["Plugins.Misc.UserNotifications.Preferences.TopicsDesc"] = "انواع موضوعاتی که مایل به دریافت آنها هستید را انتخاب نمایید.",
            ["Plugins.Misc.UserNotifications.Preferences.OrderUpdates"] = "سفارش‌ها و مرسولات",
            ["Plugins.Misc.UserNotifications.Preferences.OrderUpdatesDesc"] = "تأیید سفارش، فاکتور، رهگیری مرسوله و تحویل کالا.",
            ["Plugins.Misc.UserNotifications.Preferences.Promotions"] = "تخفیف‌ها و حراجی‌ها",
            ["Plugins.Misc.UserNotifications.Preferences.PromotionsDesc"] = "کوپن‌های تخفیف اختصاصی، یادآور سبد خرید و حراج‌های ویژه.",
            ["Plugins.Misc.UserNotifications.Preferences.Announcements"] = "اطلاعیه‌های فروشگاه",
            ["Plugins.Misc.UserNotifications.Preferences.AnnouncementsDesc"] = "ساعات کاری، خدمات جدید و اخبار مهم فروشگاه.",
            ["Plugins.Misc.UserNotifications.StartShopping"] = "شروع خرید",
            ["Plugins.Misc.UserNotifications.Announcement"] = "اطلاعیه"
        };

        foreach (var lang in languages)
        {
            var isPersian = lang.LanguageCulture.StartsWith("fa", StringComparison.OrdinalIgnoreCase);
            var resources = isPersian ? faResources : enResources;
            await _localizationService.AddOrUpdateLocaleResourceAsync(resources, lang.Id);
        }
    }

    public override async Task UninstallAsync()
    {
        var taskType = "Nop.Plugin.Misc.UserNotifications.Tasks.ProcessNotificationWorkflowsTask, Nop.Plugin.Misc.UserNotifications";
        var task = await _scheduleTaskService.GetTaskByTypeAsync(taskType);
        if (task != null)
        {
            await _scheduleTaskService.DeleteTaskAsync(task);
        }

        await _settingService.DeleteSettingAsync<FarazSmsSettings>();
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.UserNotifications");

        await base.UninstallAsync();
    }

    public async Task ManageSiteMapAsync(AdminMenuItem rootNode)
    {
        var menu = rootNode.GetItemBySystemName("Promotions");
        if (menu != null)
        {
            var parentItem = new AdminMenuItem
            {
                SystemName = "Misc.UserNotifications.Parent",
                Title = await _localizationService.GetResourceAsync("Plugins.Misc.UserNotifications.Workflows") ?? "Notifications & Workflows",
                IconClass = "far fa-bell",
                Visible = true
            };

            parentItem.ChildNodes.Add(new AdminMenuItem
            {
                SystemName = "Misc.UserNotifications.Workflows",
                Title = await _localizationService.GetResourceAsync("Plugins.Misc.UserNotifications.Workflows") ?? "Automated Workflows",
                Url = "/Admin/UserNotifications/Workflows",
                IconClass = "far fa-circle",
                Visible = true
            });

            parentItem.ChildNodes.Add(new AdminMenuItem
            {
                SystemName = "Misc.UserNotifications.Announcements",
                Title = await _localizationService.GetResourceAsync("Plugins.Misc.UserNotifications.Announcements") ?? "Announcements",
                Url = "/Admin/UserNotifications/List",
                IconClass = "far fa-circle",
                Visible = true
            });

            parentItem.ChildNodes.Add(new AdminMenuItem
            {
                SystemName = "Misc.UserNotifications.FarazSms",
                Title = await _localizationService.GetResourceAsync("Plugins.Misc.UserNotifications.FarazSms") ?? "FarazSMS Config",
                Url = "/Admin/UserNotifications/FarazSms",
                IconClass = "far fa-circle",
                Visible = true
            });

            parentItem.ChildNodes.Add(new AdminMenuItem
            {
                SystemName = "Misc.UserNotifications.Queue",
                Title = await _localizationService.GetResourceAsync("Plugins.Misc.UserNotifications.Queue") ?? "Delivery Logs",
                Url = "/Admin/UserNotifications/Queue",
                IconClass = "far fa-circle",
                Visible = true
            });

            menu.ChildNodes.Add(parentItem);
        }
        await Task.CompletedTask;
    }

    public bool HideInWidgetList => false;
}
