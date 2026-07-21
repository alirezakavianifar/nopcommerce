using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.GroupPurchase.Domain;
using Nop.Plugin.Misc.GroupPurchase.Models;
using Nop.Plugin.Misc.GroupPurchase.Services;
using Nop.Services.Customers;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.GroupPurchase.Controllers;

[AutoValidateAntiforgeryToken]
[CheckLanguageSeoCode(true)]
public class CustomerDashboardController : BasePluginController
{
    private readonly IWalletService _walletService;
    private readonly ILotteryService _lotteryService;
    private readonly IWorkContext _workContext;
    private readonly ICustomerService _customerService;
    private readonly IGroupPurchaseService _groupPurchaseService;

    public CustomerDashboardController(
        IWalletService walletService,
        ILotteryService lotteryService,
        IWorkContext workContext,
        ICustomerService customerService,
        IGroupPurchaseService groupPurchaseService)
    {
        _walletService = walletService;
        _lotteryService = lotteryService;
        _workContext = workContext;
        _customerService = customerService;
        _groupPurchaseService = groupPurchaseService;
    }

    public virtual async Task<IActionResult> Wallet()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || !await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var regularBalance = await _walletService.GetBalanceAsync(customer.Id, WalletType.Regular);
        var groupRewardBalance = await _walletService.GetBalanceAsync(customer.Id, WalletType.GroupReward);

        var model = new CustomerWalletModel
        {
            RegularBalance = regularBalance,
            GroupRewardBalance = groupRewardBalance
        };

        return View("~/Plugins/Misc.GroupPurchase/Views/CustomerDashboard/Wallet.cshtml", model);
    }

    public virtual async Task<IActionResult> Lottery()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || !await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var totalPoints = await _lotteryService.GetTotalPointsAsync(customer.Id);

        var model = new CustomerLotteryModel
        {
            TotalPoints = totalPoints
        };

        return View("~/Plugins/Misc.GroupPurchase/Views/CustomerDashboard/Lottery.cshtml", model);
    }

    public virtual async Task<IActionResult> LeaderGroups()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || !await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var groups = await _groupPurchaseService.GetLeaderGroupsAsync(customer.Id);
        
        var modelList = new List<CustomerLeaderGroupModel>();
        foreach (var g in groups)
        {
            modelList.Add(new CustomerLeaderGroupModel
            {
                Id = g.Id,
                UniqueCode = g.UniqueCode,
                Status = g.Status.ToString(),
                CreatedOnUtc = g.CreatedOnUtc,
                DeliveryCity = g.DeliveryCity,
                MembersCount = 1
            });
        }

        var model = new CustomerLeaderGroupListModel { Data = modelList };

        return View("~/Plugins/Misc.GroupPurchase/Views/CustomerDashboard/LeaderGroups.cshtml", model);
    }

    public virtual async Task<IActionResult> SubgroupHistory()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || !await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var history = await _groupPurchaseService.GetSubgroupHistoryAsync(customer.Id);
        
        var modelList = new List<CustomerSubgroupModel>();
        foreach (var h in history)
        {
            var p = await _groupPurchaseService.GetGroupPurchaseByIdAsync(h.GroupPurchaseId);
            string leaderEmail = "Hidden";
            
            if (h.VisibilityType == VisibilityType.Full)
            {
                var leader = await _customerService.GetCustomerByIdAsync(p.LeaderCustomerId);
                leaderEmail = leader?.Email ?? "Unknown";
            }

            modelList.Add(new CustomerSubgroupModel
            {
                Id = h.Id,
                UniqueCode = p?.UniqueCode,
                Status = p?.Status.ToString(),
                JoinedOnUtc = h.AcceptedOnUtc ?? DateTime.UtcNow,
                VisibilityType = h.VisibilityType,
                LeaderEmail = leaderEmail
            });
        }

        var model = new CustomerSubgroupHistoryListModel { Data = modelList };

        return View("~/Plugins/Misc.GroupPurchase/Views/CustomerDashboard/SubgroupHistory.cshtml", model);
    }
}
