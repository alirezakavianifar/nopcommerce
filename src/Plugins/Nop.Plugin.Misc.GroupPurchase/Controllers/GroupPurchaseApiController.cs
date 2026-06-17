using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.GroupPurchase.Domain;
using Nop.Plugin.Misc.GroupPurchase.Models;
using Nop.Plugin.Misc.GroupPurchase.Services;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.GroupPurchase.Controllers;

[Route("api/group-purchase")]
[ApiController]
public class GroupPurchaseApiController : ControllerBase
{
    private readonly IGroupPurchaseService _groupPurchaseService;
    private readonly IWalletService _walletService;
    private readonly ILotteryService _lotteryService;
    private readonly ICustomerService _customerService;
    private readonly IWorkContext _workContext;

    public GroupPurchaseApiController(
        IGroupPurchaseService groupPurchaseService,
        IWalletService walletService,
        ILotteryService lotteryService,
        ICustomerService customerService,
        IWorkContext workContext)
    {
        _groupPurchaseService = groupPurchaseService;
        _walletService = walletService;
        _lotteryService = lotteryService;
        _customerService = customerService;
        _workContext = workContext;
    }

    private async Task<Core.Domain.Customers.Customer> GetAuthenticatedCustomerAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || !await _customerService.IsRegisteredAsync(customer))
            return null;
        return customer;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateGroupPurchase()
    {
        var customer = await GetAuthenticatedCustomerAsync();
        if (customer == null)
            return Unauthorized();

        var groupPurchase = await _groupPurchaseService.CreateGroupPurchaseAsync(customer);
        return Ok(new { success = true, code = groupPurchase.UniqueCode });
    }

    [HttpPost("join/{code}")]
    public async Task<IActionResult> JoinGroupPurchase(string code)
    {
        var customer = await GetAuthenticatedCustomerAsync();
        if (customer == null)
            return Unauthorized();

        var memberSize = await _groupPurchaseService.JoinGroupPurchaseAsync(customer, code);
        if (memberSize == null)
            return BadRequest(new { success = false, message = "Invalid or expired code." });

        return Ok(new { success = true });
    }

    [HttpGet("wallet")]
    public async Task<IActionResult> GetWallet()
    {
        var customer = await GetAuthenticatedCustomerAsync();
        if (customer == null)
            return Unauthorized();

        var regularBalance = await _walletService.GetBalanceAsync(customer.Id, WalletType.Regular);
        var groupRewardBalance = await _walletService.GetBalanceAsync(customer.Id, WalletType.GroupReward);

        return Ok(new { regularBalance, groupRewardBalance });
    }

    [HttpGet("lottery")]
    public async Task<IActionResult> GetLotteryPoints()
    {
        var customer = await GetAuthenticatedCustomerAsync();
        if (customer == null)
            return Unauthorized();

        var totalPoints = await _lotteryService.GetTotalPointsAsync(customer.Id);
        return Ok(new { totalPoints });
    }

    [HttpGet("leader-groups")]
    public async Task<IActionResult> GetLeaderGroups()
    {
        var customer = await GetAuthenticatedCustomerAsync();
        if (customer == null)
            return Unauthorized();

        var groups = await _groupPurchaseService.GetLeaderGroupsAsync(customer.Id);
        var response = groups.Select(g => new
        {
            g.Id,
            g.UniqueCode,
            Status = g.Status.ToString(),
            g.CreatedOnUtc,
            g.DeliveryCity,
            MembersCount = 1 // Simplified for now
        });

        return Ok(response);
    }

    [HttpGet("subgroup-history")]
    public async Task<IActionResult> GetSubgroupHistory()
    {
        var customer = await GetAuthenticatedCustomerAsync();
        if (customer == null)
            return Unauthorized();

        var history = await _groupPurchaseService.GetSubgroupHistoryAsync(customer.Id);
        var response = new List<object>();

        foreach (var h in history)
        {
            var p = await _groupPurchaseService.GetGroupPurchaseByIdAsync(h.GroupPurchaseId);
            string leaderEmail = "Hidden";

            if (h.VisibilityType == VisibilityType.Full)
            {
                var leader = await _customerService.GetCustomerByIdAsync(p.LeaderCustomerId);
                leaderEmail = leader?.Email ?? "Unknown";
            }

            response.Add(new
            {
                h.Id,
                p?.UniqueCode,
                Status = p?.Status.ToString(),
                JoinedOnUtc = h.AcceptedOnUtc ?? DateTime.UtcNow,
                VisibilityType = h.VisibilityType.ToString(),
                LeaderEmail = leaderEmail
            });
        }

        return Ok(response);
    }
}
