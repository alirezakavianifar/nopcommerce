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
[Route("Admin/UserNotifications")]
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
    protected readonly Nop.Services.Customers.ICustomerService _customerService;

    public UserNotificationsController(
        IUserNotificationService notificationService,
        IWorkflowEngineService workflowEngineService,
        ISettingService settingService,
        FarazSmsSettings farazSmsSettings,
        ILocalizationService localizationService,
        ILanguageService languageService,
        INotificationService nopNotificationService,
        IPermissionService permissionService,
        Nop.Services.Customers.ICustomerService customerService)
    {
        _notificationService = notificationService;
        _workflowEngineService = workflowEngineService;
        _settingService = settingService;
        _farazSmsSettings = farazSmsSettings;
        _localizationService = localizationService;
        _languageService = languageService;
        _nopNotificationService = nopNotificationService;
        _permissionService = permissionService;
        _customerService = customerService;
    }

    private async Task EnsureLocaleResourcesAsync()
    {
        await Task.CompletedTask;
    }

    #region Announcements

    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_VIEW)]
    [Route("List")]
    public virtual async Task<IActionResult> List()
    {
        await EnsureLocaleResourcesAsync();
        var model = new AnnouncementSearchModel();
        return View("~/Plugins/Misc.UserNotifications/Views/Admin/List.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_VIEW)]
    [Route("List")]
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
                IsPublished = a.IsPublished,
                CustomerRoleId = a.CustomerRoleId
            });
        });

        return Json(model);
    }

    protected virtual async Task PrepareAnnouncementModelAsync(AnnouncementModel model)
    {
        model.AvailableCustomerRoles.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Text = await _localizationService.GetResourceAsync("Admin.Common.All") ?? "All Roles",
            Value = "0"
        });

        var roles = await _customerService.GetAllCustomerRolesAsync();
        foreach (var role in roles)
        {
            model.AvailableCustomerRoles.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = role.Name,
                Value = role.Id.ToString(),
                Selected = model.CustomerRoleId == role.Id
            });
        }
    }

    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    [Route("Create")]
    public virtual async Task<IActionResult> Create()
    {
        var model = new AnnouncementModel();
        await PrepareAnnouncementModelAsync(model);
        return View("~/Plugins/Misc.UserNotifications/Views/Admin/Create.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    [Route("Create")]
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
                CustomerRoleId = model.CustomerRoleId > 0 ? model.CustomerRoleId : null,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _notificationService.InsertAnnouncementAsync(announcement);
            _nopNotificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Misc.UserNotifications.Added"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = announcement.Id });
        }

        await PrepareAnnouncementModelAsync(model);
        return View("~/Plugins/Misc.UserNotifications/Views/Admin/Create.cshtml", model);
    }

    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    [Route("Edit/{id}")]
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
            IsPublished = announcement.IsPublished,
            CustomerRoleId = announcement.CustomerRoleId
        };

        await PrepareAnnouncementModelAsync(model);
        return View("~/Plugins/Misc.UserNotifications/Views/Admin/Edit.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    [Route("Edit/{id}")]
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
            announcement.CustomerRoleId = model.CustomerRoleId > 0 ? model.CustomerRoleId : null;

            await _notificationService.UpdateAnnouncementAsync(announcement);
            _nopNotificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Misc.UserNotifications.Updated"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = announcement.Id });
        }

        await PrepareAnnouncementModelAsync(model);
        return View("~/Plugins/Misc.UserNotifications/Views/Admin/Edit.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    [Route("Delete/{id}")]
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
    [Route("Workflows")]
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
    [Route("CreateWorkflow")]
    public virtual async Task<IActionResult> CreateWorkflow()
    {
        await EnsureLocaleResourcesAsync();
        var model = new WorkflowModel { IsActive = true };
        return View("~/Plugins/Misc.UserNotifications/Views/Admin/Workflows/Create.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    [Route("CreateWorkflow")]
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
    [Route("EditWorkflow/{id}")]
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
    [Route("SaveWorkflowStep")]
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
    [Route("FarazSms")]
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
    [Route("FarazSms")]
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
    [Route("Queue")]
    public virtual async Task<IActionResult> Queue()
    {
        await EnsureLocaleResourcesAsync();
        var items = await _workflowEngineService.GetQueueItemsAsync();
        return View("~/Plugins/Misc.UserNotifications/Views/Admin/Queue/List.cshtml", items);
    }

    #endregion
}
