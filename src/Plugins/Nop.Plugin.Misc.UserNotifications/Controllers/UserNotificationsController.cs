using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.UserNotifications.Domain;
using Nop.Plugin.Misc.UserNotifications.Models;
using Nop.Plugin.Misc.UserNotifications.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.UserNotifications.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class UserNotificationsController : BasePluginController
{
    protected readonly IUserNotificationService _notificationService;
    protected readonly IWorkflowEngineService _workflowEngineService;
    protected readonly ISettingService _settingService;
    protected readonly FarazSmsSettings _farazSmsSettings;
    protected readonly ILocalizationService _localizationService;
    protected readonly ILanguageService _languageService;
    protected readonly INotificationService _nopNotificationService;
    protected readonly IPermissionService _permissionService;

    public UserNotificationsController(
        IUserNotificationService notificationService,
        IWorkflowEngineService workflowEngineService,
        ISettingService settingService,
        FarazSmsSettings farazSmsSettings,
        ILocalizationService localizationService,
        ILanguageService languageService,
        INotificationService nopNotificationService,
        IPermissionService permissionService)
    {
        _notificationService = notificationService;
        _workflowEngineService = workflowEngineService;
        _settingService = settingService;
        _farazSmsSettings = farazSmsSettings;
        _localizationService = localizationService;
        _languageService = languageService;
        _nopNotificationService = nopNotificationService;
        _permissionService = permissionService;
    }

    private async Task EnsureLocaleResourcesAsync()
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

    #region Announcements

    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_VIEW)]
    public virtual async Task<IActionResult> List()
    {
        await EnsureLocaleResourcesAsync();
        var model = new AnnouncementSearchModel();
        return View("~/Plugins/Misc.UserNotifications/Views/Admin/List.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_VIEW)]
    public virtual async Task<IActionResult> List(AnnouncementSearchModel searchModel)
    {
        var announcements = await _notificationService.GetAllAnnouncementsAsync(searchModel.Page - 1, searchModel.PageSize, true);
        var model = new AnnouncementListModel().PrepareToGrid(searchModel, announcements, () =>
        {
            return announcements.Select(a => new AnnouncementModel
            {
                Id = a.Id,
                Title = a.Title,
                Body = a.Body,
                StartDateUtc = a.StartDateUtc,
                EndDateUtc = a.EndDateUtc,
                IsPublished = a.IsPublished
            });
        });

        return Json(model);
    }

    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Create()
    {
        var model = new AnnouncementModel();
        return View("~/Plugins/Misc.UserNotifications/Views/Admin/Create.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Create(AnnouncementModel model, bool continueEditing)
    {
        if (ModelState.IsValid)
        {
            var announcement = new NotificationAnnouncement
            {
                Title = model.Title,
                Body = model.Body,
                StartDateUtc = model.StartDateUtc,
                EndDateUtc = model.EndDateUtc,
                IsPublished = model.IsPublished,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _notificationService.InsertAnnouncementAsync(announcement);
            _nopNotificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Misc.UserNotifications.Added"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = announcement.Id });
        }

        return View("~/Plugins/Misc.UserNotifications/Views/Admin/Create.cshtml", model);
    }

    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Edit(int id)
    {
        var announcement = await _notificationService.GetAnnouncementByIdAsync(id);
        if (announcement == null)
            return RedirectToAction("List");

        var model = new AnnouncementModel
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Body = announcement.Body,
            StartDateUtc = announcement.StartDateUtc,
            EndDateUtc = announcement.EndDateUtc,
            IsPublished = announcement.IsPublished
        };

        return View("~/Plugins/Misc.UserNotifications/Views/Admin/Edit.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Edit(AnnouncementModel model, bool continueEditing)
    {
        if (ModelState.IsValid)
        {
            var announcement = await _notificationService.GetAnnouncementByIdAsync(model.Id);
            if (announcement == null)
                return RedirectToAction("List");

            announcement.Title = model.Title;
            announcement.Body = model.Body;
            announcement.StartDateUtc = model.StartDateUtc;
            announcement.EndDateUtc = model.EndDateUtc;
            announcement.IsPublished = model.IsPublished;

            await _notificationService.UpdateAnnouncementAsync(announcement);
            _nopNotificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Misc.UserNotifications.Updated"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = announcement.Id });
        }

        return View("~/Plugins/Misc.UserNotifications/Views/Admin/Edit.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var announcement = await _notificationService.GetAnnouncementByIdAsync(id);
        if (announcement != null)
        {
            await _notificationService.DeleteAnnouncementAsync(announcement);
            _nopNotificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Misc.UserNotifications.Deleted"));
        }

        return RedirectToAction("List");
    }

    #endregion

    #region Automated Workflows

    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_VIEW)]
    public virtual async Task<IActionResult> Workflows()
    {
        await EnsureLocaleResourcesAsync();
        var workflows = await _workflowEngineService.GetAllWorkflowsAsync();
        var model = workflows.Select(w => new WorkflowModel
        {
            Id = w.Id,
            Name = w.Name,
            TriggerTypeId = w.TriggerTypeId,
            IsActive = w.IsActive,
            CreatedOnUtc = w.CreatedOnUtc
        }).ToList();

        return View("~/Plugins/Misc.UserNotifications/Views/Admin/Workflows/List.cshtml", model);
    }

    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> CreateWorkflow()
    {
        await EnsureLocaleResourcesAsync();
        var model = new WorkflowModel { IsActive = true };
        return View("~/Plugins/Misc.UserNotifications/Views/Admin/Workflows/Create.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> CreateWorkflow(WorkflowModel model)
    {
        if (ModelState.IsValid)
        {
            var workflow = new NotificationWorkflow
            {
                Name = model.Name,
                TriggerTypeId = model.TriggerTypeId,
                IsActive = model.IsActive,
                CreatedOnUtc = DateTime.UtcNow
            };
            await _workflowEngineService.InsertWorkflowAsync(workflow);
            _nopNotificationService.SuccessNotification("Workflow created successfully.");
            return RedirectToAction("EditWorkflow", new { id = workflow.Id });
        }
        return View("~/Plugins/Misc.UserNotifications/Views/Admin/Workflows/Create.cshtml", model);
    }

    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> EditWorkflow(int id)
    {
        await EnsureLocaleResourcesAsync();
        var workflow = await _workflowEngineService.GetWorkflowByIdAsync(id);
        if (workflow == null)
            return RedirectToAction("Workflows");

        var steps = await _workflowEngineService.GetWorkflowStepsAsync(workflow.Id);
        var model = new WorkflowModel
        {
            Id = workflow.Id,
            Name = workflow.Name,
            TriggerTypeId = workflow.TriggerTypeId,
            IsActive = workflow.IsActive,
            CreatedOnUtc = workflow.CreatedOnUtc,
            Steps = steps.Select(s => new WorkflowStepModel
            {
                Id = s.Id,
                WorkflowId = s.WorkflowId,
                StepOrder = s.StepOrder,
                DelayMinutes = s.DelayMinutes,
                SendEmail = s.SendEmail,
                SendSms = s.SendSms,
                SendPopUp = s.SendPopUp,
                SendInbox = s.SendInbox,
                SubjectTemplate = s.SubjectTemplate,
                BodyTemplate = s.BodyTemplate,
                GenerateDiscountCode = s.GenerateDiscountCode,
                DiscountPercentage = s.DiscountPercentage,
                SmsPatternCode = s.SmsPatternCode,
                IsActive = s.IsActive
            }).ToList()
        };

        return View("~/Plugins/Misc.UserNotifications/Views/Admin/Workflows/Edit.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> SaveWorkflowStep(WorkflowStepModel stepModel)
    {
        if (stepModel.Id > 0)
        {
            var step = await _workflowEngineService.GetStepByIdAsync(stepModel.Id);
            if (step != null)
            {
                step.DelayMinutes = stepModel.DelayMinutes;
                step.SendEmail = stepModel.SendEmail;
                step.SendSms = stepModel.SendSms;
                step.SendPopUp = stepModel.SendPopUp;
                step.SendInbox = stepModel.SendInbox;
                step.SubjectTemplate = stepModel.SubjectTemplate;
                step.BodyTemplate = stepModel.BodyTemplate;
                step.GenerateDiscountCode = stepModel.GenerateDiscountCode;
                step.DiscountPercentage = stepModel.DiscountPercentage;
                step.SmsPatternCode = stepModel.SmsPatternCode;
                step.IsActive = stepModel.IsActive;

                await _workflowEngineService.UpdateStepAsync(step);
            }
        }
        else
        {
            var newStep = new NotificationWorkflowStep
            {
                WorkflowId = stepModel.WorkflowId,
                StepOrder = stepModel.StepOrder,
                DelayMinutes = stepModel.DelayMinutes,
                SendEmail = stepModel.SendEmail,
                SendSms = stepModel.SendSms,
                SendPopUp = stepModel.SendPopUp,
                SendInbox = stepModel.SendInbox,
                SubjectTemplate = stepModel.SubjectTemplate,
                BodyTemplate = stepModel.BodyTemplate,
                GenerateDiscountCode = stepModel.GenerateDiscountCode,
                DiscountPercentage = stepModel.DiscountPercentage,
                SmsPatternCode = stepModel.SmsPatternCode,
                IsActive = true
            };
            await _workflowEngineService.InsertStepAsync(newStep);
        }

        _nopNotificationService.SuccessNotification("Workflow step saved successfully.");
        return RedirectToAction("EditWorkflow", new { id = stepModel.WorkflowId });
    }

    #endregion

    #region FarazSMS

    [CheckPermission(StandardPermission.Configuration.MANAGE_SETTINGS)]
    public virtual async Task<IActionResult> FarazSms()
    {
        await EnsureLocaleResourcesAsync();
        var model = new FarazSmsSettingsModel
        {
            Enabled = _farazSmsSettings.Enabled,
            ApiUrl = _farazSmsSettings.ApiUrl,
            ApiKey = _farazSmsSettings.ApiKey,
            SenderNumber = _farazSmsSettings.SenderNumber,
            DefaultPatternCode = _farazSmsSettings.DefaultPatternCode
        };

        return View("~/Plugins/Misc.UserNotifications/Views/Admin/FarazSms/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_SETTINGS)]
    public virtual async Task<IActionResult> FarazSms(FarazSmsSettingsModel model)
    {
        if (ModelState.IsValid)
        {
            _farazSmsSettings.Enabled = model.Enabled;
            _farazSmsSettings.ApiUrl = model.ApiUrl;
            _farazSmsSettings.ApiKey = model.ApiKey;
            _farazSmsSettings.SenderNumber = model.SenderNumber;
            _farazSmsSettings.DefaultPatternCode = model.DefaultPatternCode;

            await _settingService.SaveSettingAsync(_farazSmsSettings);
            _nopNotificationService.SuccessNotification("FarazSMS settings updated successfully.");
        }

        return View("~/Plugins/Misc.UserNotifications/Views/Admin/FarazSms/Configure.cshtml", model);
    }

    #endregion

    #region Queue Logs

    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_VIEW)]
    public virtual async Task<IActionResult> Queue()
    {
        await EnsureLocaleResourcesAsync();
        var items = await _workflowEngineService.GetQueueItemsAsync();
        return View("~/Plugins/Misc.UserNotifications/Views/Admin/Queue/List.cshtml", items);
    }

    #endregion
}
