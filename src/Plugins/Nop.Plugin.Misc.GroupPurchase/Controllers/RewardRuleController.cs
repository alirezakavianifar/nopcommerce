using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.GroupPurchase.Domain;
using Nop.Plugin.Misc.GroupPurchase.Models;
using Nop.Plugin.Misc.GroupPurchase.Services;
using Nop.Services.Security;

using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.DataTables;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.GroupPurchase.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class RewardRuleController : BasePluginController
{
    private readonly IRewardRuleService _rewardRuleService;

    public RewardRuleController(IRewardRuleService rewardRuleService)
    {
        _rewardRuleService = rewardRuleService;
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual IActionResult List()
    {
        var model = new RewardRuleSearchModel();
        return View("~/Plugins/Misc.GroupPurchase/Views/RewardRule/List.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> ListData(RewardRuleSearchModel searchModel)
    {
        var rewardRules = await _rewardRuleService.GetAllRewardRulesAsync(searchModel.Page - 1, searchModel.PageSize);

        var model = new RewardRuleListModel().PrepareToGrid(searchModel, rewardRules, () =>
        {
            return rewardRules.Select(rr => new RewardRuleModel
            {
                Id = rr.Id,
                TargetRoleName = rr.TargetRole.ToString(),
                RewardTypeName = rr.RewardType.ToString(),
                CalculationTypeName = rr.CalculationType.ToString(),
                Value = rr.Value,
                CategoryId = rr.CategoryId,
                MinCartAmount = rr.MinCartAmount,
                MinMembers = rr.MinMembers
            });
        });

        return Json(model);
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual IActionResult Create()
    {
        var model = new RewardRuleModel();
        return View("~/Plugins/Misc.GroupPurchase/Views/RewardRule/Create.cshtml", model);
    }

    [HttpPost]
    [ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> Create(RewardRuleModel model, bool continueEditing)
    {
        if (ModelState.IsValid)
        {
            var rewardRule = new RewardRule
            {
                TargetRoleId = model.TargetRoleId,
                RewardTypeId = model.RewardTypeId,
                CalculationTypeId = model.CalculationTypeId,
                Value = model.Value,
                CategoryId = model.CategoryId,
                MinCartAmount = model.MinCartAmount,
                MinMembers = model.MinMembers
            };

            await _rewardRuleService.InsertRewardRuleAsync(rewardRule);

            if (continueEditing)
                return RedirectToAction("Edit", new { id = rewardRule.Id });

            return RedirectToAction("List");
        }

        return View("~/Plugins/Misc.GroupPurchase/Views/RewardRule/Create.cshtml", model);
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> Edit(int id)
    {
        var rewardRule = await _rewardRuleService.GetRewardRuleByIdAsync(id);
        if (rewardRule == null)
            return RedirectToAction("List");

        var model = new RewardRuleModel
        {
            Id = rewardRule.Id,
            TargetRoleId = rewardRule.TargetRoleId,
            RewardTypeId = rewardRule.RewardTypeId,
            CalculationTypeId = rewardRule.CalculationTypeId,
            Value = rewardRule.Value,
            CategoryId = rewardRule.CategoryId,
            MinCartAmount = rewardRule.MinCartAmount,
            MinMembers = rewardRule.MinMembers
        };

        return View("~/Plugins/Misc.GroupPurchase/Views/RewardRule/Edit.cshtml", model);
    }

    [HttpPost]
    [ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> Edit(RewardRuleModel model, bool continueEditing)
    {
        var rewardRule = await _rewardRuleService.GetRewardRuleByIdAsync(model.Id);
        if (rewardRule == null)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            rewardRule.TargetRoleId = model.TargetRoleId;
            rewardRule.RewardTypeId = model.RewardTypeId;
            rewardRule.CalculationTypeId = model.CalculationTypeId;
            rewardRule.Value = model.Value;
            rewardRule.CategoryId = model.CategoryId;
            rewardRule.MinCartAmount = model.MinCartAmount;
            rewardRule.MinMembers = model.MinMembers;

            await _rewardRuleService.UpdateRewardRuleAsync(rewardRule);

            if (continueEditing)
                return RedirectToAction("Edit", new { id = rewardRule.Id });

            return RedirectToAction("List");
        }

        return View("~/Plugins/Misc.GroupPurchase/Views/RewardRule/Edit.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var rewardRule = await _rewardRuleService.GetRewardRuleByIdAsync(id);
        if (rewardRule == null)
            return RedirectToAction("List");

        await _rewardRuleService.DeleteRewardRuleAsync(rewardRule);

        return RedirectToAction("List");
    }
}
