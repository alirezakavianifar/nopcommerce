using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.GroupPurchase.Models;
using Nop.Plugin.Misc.GroupPurchase.Services;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.GroupPurchase.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class CustomerClubAdminController : BasePluginController
{
    private readonly ICustomerClubService _customerClubService;
    private readonly INotificationService _notificationService;

    public CustomerClubAdminController(
        ICustomerClubService customerClubService,
        INotificationService notificationService)
    {
        _customerClubService = customerClubService;
        _notificationService = notificationService;
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> Configure()
    {
        var settings = await _customerClubService.GetSettingsAsync();

        var model = new CustomerClubModel
        {
            PointsToLotteryChanceRatio = settings.PointsToLotteryChanceRatio,
            PointsToCurrencyRatio = settings.PointsToCurrencyRatio,
            EnableDualConversion = settings.EnableDualConversion,
            DefaultMinRewardAmount = settings.DefaultMinRewardAmount,
            DefaultMaxRewardAmount = settings.DefaultMaxRewardAmount
        };

        return View("~/Plugins/Misc.GroupPurchase/Views/CustomerClub/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public virtual async Task<IActionResult> Configure(CustomerClubModel model)
    {
        if (ModelState.IsValid)
        {
            var settings = await _customerClubService.GetSettingsAsync();
            settings.PointsToLotteryChanceRatio = model.PointsToLotteryChanceRatio;
            settings.PointsToCurrencyRatio = model.PointsToCurrencyRatio;
            settings.EnableDualConversion = model.EnableDualConversion;
            settings.DefaultMinRewardAmount = model.DefaultMinRewardAmount;
            settings.DefaultMaxRewardAmount = model.DefaultMaxRewardAmount;

            await _customerClubService.SaveSettingsAsync(settings);
            _notificationService.SuccessNotification("Customer club settings updated successfully.");

            return RedirectToAction("Configure");
        }

        return View("~/Plugins/Misc.GroupPurchase/Views/CustomerClub/Configure.cshtml", model);
    }
}
