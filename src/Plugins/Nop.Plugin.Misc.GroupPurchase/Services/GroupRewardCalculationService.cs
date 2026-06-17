using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.GroupPurchase.Domain;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.GroupPurchase.Services;

/// <summary>
/// Group reward calculation service
/// </summary>
public class GroupRewardCalculationService : IGroupRewardCalculationService
{
    #region Fields

    private readonly IRewardRuleService _rewardRuleService;
    private readonly IRepository<GroupPurchaseReward> _groupPurchaseRewardRepository;
    private readonly IRepository<GroupPurchaseMember> _groupPurchaseMemberRepository;
    private readonly IWalletService _walletService;
    private readonly ILotteryService _lotteryService;

    #endregion

    #region Ctor

    public GroupRewardCalculationService(
        IRewardRuleService rewardRuleService,
        IRepository<GroupPurchaseReward> groupPurchaseRewardRepository,
        IRepository<GroupPurchaseMember> groupPurchaseMemberRepository,
        IWalletService walletService,
        ILotteryService lotteryService)
    {
        _rewardRuleService = rewardRuleService;
        _groupPurchaseRewardRepository = groupPurchaseRewardRepository;
        _groupPurchaseMemberRepository = groupPurchaseMemberRepository;
        _walletService = walletService;
        _lotteryService = lotteryService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Calculate and apply reward for a group purchase order placement
    /// </summary>
    /// <param name="order">The order</param>
    /// <param name="groupPurchase">The group purchase</param>
    /// <param name="member">The member</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task CalculateAndApplyRewardAsync(Order order, Domain.GroupPurchase groupPurchase, GroupPurchaseMember member)
    {
        if (order == null || groupPurchase == null || member == null)
            return;

        // Determine role
        var targetRole = member.IsLeader ? RewardRuleTargetRole.Leader : RewardRuleTargetRole.Subgroup;

        // Get group size
        var groupMembers = await _groupPurchaseMemberRepository.GetAllAsync(query =>
            query.Where(m => m.GroupPurchaseId == groupPurchase.Id));
        var groupSize = groupMembers.Count;

        // Get all rules
        var allRules = await _rewardRuleService.GetAllRewardRulesAsync();

        // Applicable rules
        var rules = allRules.Where(r => 
            r.TargetRole == targetRole && 
            r.MinCartAmount <= order.OrderTotal &&
            r.MinMembers <= groupSize
        ).ToList();

        foreach (var rule in rules)
        {
            decimal rewardAmount = 0m;

            switch (rule.CalculationType)
            {
                case CalculationType.Fixed:
                    rewardAmount = rule.Value;
                    break;
                case CalculationType.PercentageOfCartTotal:
                    rewardAmount = (rule.Value / 100m) * order.OrderTotal;
                    break;
                case CalculationType.PercentageOfNetProfit:
                    // Assuming Net Profit calculation is simply proportional or specific custom logic here.
                    // For now, calculating based on OrderSubTotalExclTax as a proxy, or leaving to custom extension
                    // Usually implies requiring product cost. Let's fallback to percentage of subtotal.
                    rewardAmount = (rule.Value / 100m) * order.OrderSubtotalExclTax;
                    break;
            }

            if (rewardAmount > 0)
            {
                var reward = new GroupPurchaseReward
                {
                    GroupPurchaseId = groupPurchase.Id,
                    CustomerId = order.CustomerId,
                    RewardTypeId = rule.RewardTypeId,
                    CalculationTypeId = rule.CalculationTypeId,
                    CategoryId = rule.CategoryId,
                    Amount = rewardAmount,
                    CreatedOnUtc = DateTime.UtcNow
                };

                await _groupPurchaseRewardRepository.InsertAsync(reward);
                
                // Apply to wallet if WalletCredit or LotteryPoints
                if (rule.RewardType == RewardType.WalletCredit)
                {
                    await _walletService.AddTransactionAsync(
                        order.CustomerId,
                        WalletType.GroupReward,
                        rewardAmount,
                        $"Group purchase reward ({groupPurchase.UniqueCode})");
                }
                else if (rule.RewardType == RewardType.LotteryPoints)
                {
                    await _lotteryService.AddPointsAsync(
                        order.CustomerId,
                        (int)rewardAmount,
                        targetRole == RewardRuleTargetRole.Leader ? LotterySource.GroupPurchaseLeader : LotterySource.GroupPurchaseMember,
                        groupPurchase.Id);
                }
            }
        }
    }

    #endregion
}
