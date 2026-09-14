using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.GroupPurchase.Domain;
using Nop.Plugin.Misc.GroupPurchase.Models;
using Nop.Plugin.Misc.GroupPurchase.Services;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.GroupPurchase.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class CommissionAdminController : BasePluginController
{
    private readonly ICommissionService _commissionService;
    private readonly IExternalCommissionProvider _externalCommissionProvider;
    private readonly INotificationService _notificationService;

    public CommissionAdminController(
        ICommissionService commissionService,
        IExternalCommissionProvider externalCommissionProvider,
        INotificationService notificationService)
    {
        _commissionService = commissionService;
        _externalCommissionProvider = externalCommissionProvider;
        _notificationService = notificationService;
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual IActionResult List()
    {
        var model = new CommissionSearchModel();
        return View("~/Plugins/Misc.GroupPurchase/Views/Commission/List.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> ListData(CommissionSearchModel searchModel)
    {
        CommissionScope? scope = searchModel.ScopeId.HasValue ? (CommissionScope)searchModel.ScopeId.Value : null;
        var commissions = await _commissionService.GetAllCommissionsAsync(scope, searchModel.Page - 1, searchModel.PageSize);

        var model = new CommissionListModel().PrepareToGrid(searchModel, commissions, () =>
        {
            return commissions.Select(c => new CommissionModel
            {
                Id = c.Id,
                CommissionScopeId = c.CommissionScopeId,
                ScopeName = c.Scope.ToString(),
                EntityId = c.EntityId,
                EntityName = c.EntityName,
                Percentage = c.Percentage
            });
        });

        return Json(model);
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual IActionResult Create()
    {
        var model = new CommissionModel();
        return View("~/Plugins/Misc.GroupPurchase/Views/Commission/Create.cshtml", model);
    }

    [HttpPost]
    [ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> Create(CommissionModel model, bool continueEditing)
    {
        if (ModelState.IsValid)
        {
            var commission = new GroupPurchaseCommission
            {
                CommissionScopeId = model.CommissionScopeId,
                EntityId = model.EntityId,
                Percentage = model.Percentage,
                EntityName = model.EntityName
            };

            await _commissionService.InsertCommissionAsync(commission);
            _notificationService.SuccessNotification("Commission percentage added successfully.");

            if (continueEditing)
                return RedirectToAction("Edit", new { id = commission.Id });

            return RedirectToAction("List");
        }

        return View("~/Plugins/Misc.GroupPurchase/Views/Commission/Create.cshtml", model);
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> Edit(int id)
    {
        var commission = await _commissionService.GetCommissionByIdAsync(id);
        if (commission == null)
            return RedirectToAction("List");

        var model = new CommissionModel
        {
            Id = commission.Id,
            CommissionScopeId = commission.CommissionScopeId,
            ScopeName = commission.Scope.ToString(),
            EntityId = commission.EntityId,
            EntityName = commission.EntityName,
            Percentage = commission.Percentage
        };

        return View("~/Plugins/Misc.GroupPurchase/Views/Commission/Edit.cshtml", model);
    }

    [HttpPost]
    [ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> Edit(CommissionModel model, bool continueEditing)
    {
        var commission = await _commissionService.GetCommissionByIdAsync(model.Id);
        if (commission == null)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            commission.CommissionScopeId = model.CommissionScopeId;
            commission.EntityId = model.EntityId;
            commission.Percentage = model.Percentage;
            commission.EntityName = model.EntityName;

            await _commissionService.UpdateCommissionAsync(commission);
            _notificationService.SuccessNotification("Commission percentage updated successfully.");

            if (continueEditing)
                return RedirectToAction("Edit", new { id = commission.Id });

            return RedirectToAction("List");
        }

        return View("~/Plugins/Misc.GroupPurchase/Views/Commission/Edit.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var commission = await _commissionService.GetCommissionByIdAsync(id);
        if (commission != null)
        {
            await _commissionService.DeleteCommissionAsync(commission);
            _notificationService.SuccessNotification("Commission percentage deleted.");
        }

        return RedirectToAction("List");
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> ImportExcel(IFormFile importexcelfile)
    {
        if (importexcelfile != null && importexcelfile.Length > 0)
        {
            using var stream = importexcelfile.OpenReadStream();
            var (imported, errors) = await _commissionService.ImportFromExcelOrCsvAsync(stream);
            _notificationService.SuccessNotification($"Import completed: {imported} records imported, {errors} errors.");
        }
        else
        {
            _notificationService.ErrorNotification("Please upload a valid Excel file.");
        }

        return RedirectToAction("List");
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> SyncExternal()
    {
        var success = await _externalCommissionProvider.SyncFromExternalSystemAsync();
        if (success)
            _notificationService.SuccessNotification("External synchronization initiated successfully.");
        else
            _notificationService.WarningNotification("External synchronization encountered warnings.");

        return RedirectToAction("List");
    }
}
