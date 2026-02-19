using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.UserNotifications.Domain;
using Nop.Plugin.Misc.UserNotifications.Models;
using Nop.Plugin.Misc.UserNotifications.Services;
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
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _nopNotificationService;
    protected readonly IPermissionService _permissionService;

    public UserNotificationsController(IUserNotificationService notificationService,
        ILocalizationService localizationService,
        INotificationService nopNotificationService,
        IPermissionService permissionService)
    {
        _notificationService = notificationService;
        _localizationService = localizationService;
        _nopNotificationService = nopNotificationService;
        _permissionService = permissionService;
    }

    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_VIEW)]
    public virtual async Task<IActionResult> List()
    {
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
                StartDateUtc = a.StartDateUtc,
                EndDateUtc = a.EndDateUtc,
                IsPublished = a.IsPublished,
                CreatedOnUtc = a.CreatedOnUtc
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
            IsPublished = announcement.IsPublished,
            CreatedOnUtc = announcement.CreatedOnUtc
        };

        return View("~/Plugins/Misc.UserNotifications/Views/Admin/Edit.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.Promotions.DISCOUNTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Edit(AnnouncementModel model, bool continueEditing)
    {
        var announcement = await _notificationService.GetAnnouncementByIdAsync(model.Id);
        if (announcement == null)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
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
        if (announcement == null)
            return RedirectToAction("List");

        await _notificationService.DeleteAnnouncementAsync(announcement);

        _nopNotificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Misc.UserNotifications.Deleted"));

        return RedirectToAction("List");
    }
}
