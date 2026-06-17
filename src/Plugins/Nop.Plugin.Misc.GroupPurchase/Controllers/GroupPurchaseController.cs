using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.GroupPurchase.Services;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.GroupPurchase.Controllers;

public class GroupPurchaseController : BasePluginController
{
    protected readonly IGroupPurchaseService _groupPurchaseService;
    protected readonly IWorkContext _workContext;

    public GroupPurchaseController(IGroupPurchaseService groupPurchaseService,
        IWorkContext workContext)
    {
        _groupPurchaseService = groupPurchaseService;
        _workContext = workContext;
    }

    [HttpPost]
    public async Task<IActionResult> ConvertToGroupPurchase()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || await _workContext.GetCurrentCustomerAsync() == null) // Double check for guest/auth
            return Challenge();

        var groupPurchase = await _groupPurchaseService.CreateGroupPurchaseAsync(customer);

        return Json(new { success = true, code = groupPurchase.UniqueCode });
    }

    [HttpPost]
    public async Task<IActionResult> JoinGroupPurchase(string code)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null)
            return Challenge();

        var memberSize = await _groupPurchaseService.JoinGroupPurchaseAsync(customer, code);
        if (memberSize == null)
            return Json(new { success = false, message = "Invalid or expired code." });

        return Json(new { success = true });
    }
}
