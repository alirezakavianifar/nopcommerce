using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.GroupPurchase.Models;
using Nop.Plugin.Misc.GroupPurchase.Services;
using Nop.Plugin.Misc.GroupPurchase.Domain;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.GroupPurchase.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class GroupPurchaseAdminController : BasePluginController
{
    protected readonly IGroupPurchaseService _groupPurchaseService;

    public GroupPurchaseAdminController(IGroupPurchaseService groupPurchaseService)
    {
        _groupPurchaseService = groupPurchaseService;
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual IActionResult List()
    {
        var model = new GroupPurchaseSearchModel();
        return View("~/Plugins/Misc.GroupPurchase/Views/Admin/List.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> ListData(GroupPurchaseSearchModel searchModel)
    {
        var groupPurchases = await _groupPurchaseService.GetAllGroupPurchasesAsync(
            searchModel.Page > 0 ? searchModel.Page - 1 : 0, 
            searchModel.PageSize > 0 ? searchModel.PageSize : 10);

        var model = new GroupPurchaseListModel().PrepareToGrid(searchModel, groupPurchases, () =>
        {
            return groupPurchases.Select(gp => new GroupPurchaseModel
            {
                Id = gp.Id,
                LeaderCustomerId = gp.LeaderCustomerId,
                UniqueCode = gp.UniqueCode,
                Status = gp.Status.ToString(),
                CreatedOnUtc = gp.CreatedOnUtc,
                DeliveryCity = gp.DeliveryCity
            });
        });

        return Json(model);
    }
}
