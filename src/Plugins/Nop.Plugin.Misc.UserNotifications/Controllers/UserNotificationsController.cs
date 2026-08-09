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
