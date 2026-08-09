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
            PublicWidgetZones.BodyStartHtmlTagAfter
        });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == PublicWidgetZones.HeaderLinksAfter)
            return typeof(NotificationsInboxViewComponent);

        if (widgetZone == PublicWidgetZones.BodyStartHtmlTagAfter)
            return typeof(PopupModalViewComponent);

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
            ["Plugins.Misc.UserNotifications.Deleted"] = "The announcement has been deleted successfully."
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
            ["Plugins.Misc.UserNotifications.Deleted"] = "اطلاعیه با موفقیت حذف شد."
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
