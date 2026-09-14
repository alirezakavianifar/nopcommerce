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
    private readonly ICommissionService _commissionService;
    private readonly Nop.Services.Orders.IOrderService _orderService;
    private readonly IProductService _productService;

    #endregion

    #region Ctor

    public GroupRewardCalculationService(
        IRewardRuleService rewardRuleService,
        IRepository<GroupPurchaseReward> groupPurchaseRewardRepository,
        IRepository<GroupPurchaseMember> groupPurchaseMemberRepository,
        IWalletService walletService,
        ILotteryService lotteryService,
        ICommissionService commissionService,
        Nop.Services.Orders.IOrderService orderService,
        IProductService productService)
    {
        _rewardRuleService = rewardRuleService;
        _groupPurchaseRewardRepository = groupPurchaseRewardRepository;
        _groupPurchaseMemberRepository = groupPurchaseMemberRepository;
        _walletService = walletService;
        _lotteryService = lotteryService;
        _commissionService = commissionService;
        _orderService = orderService;
        _productService = productService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Calculate and apply reward for a group purchase order placement
    /// </summary>
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

        // Calculate order items and net profit once if any rule uses PercentageOfNetProfit
        decimal totalNetProfit = 0m;
        bool netProfitCalculated = false;

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
                    if (!netProfitCalculated)
                    {
                        totalNetProfit = await CalculateOrderNetProfitAsync(order);
                        netProfitCalculated = true;
                    }
                    rewardAmount = (rule.Value / 100m) * totalNetProfit;
                    break;
            }

            // Apply Min / Max reward amount caps (as required by client)
            if (rule.MinRewardAmount.HasValue && rewardAmount < rule.MinRewardAmount.Value)
            {
                rewardAmount = rule.MinRewardAmount.Value;
            }

            if (rule.MaxRewardAmount.HasValue && rewardAmount > rule.MaxRewardAmount.Value)
            {
                rewardAmount = rule.MaxRewardAmount.Value;
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

    /// <summary>
    /// Calculates the net profit of an order:
    /// Gross Profit based on commission hierarchy (Product -> Category -> Vendor -> Brand -> Cost fallback)
    /// minus regular site discount and group purchase discount
    /// </summary>
    protected virtual async Task<decimal> CalculateOrderNetProfitAsync(Order order)
    {
        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
        if (orderItems == null || !orderItems.Any())
            return 0m;

        decimal totalNetProfit = 0m;
        decimal orderSubtotal = order.OrderSubtotalExclTax > 0 ? order.OrderSubtotalExclTax : 1m;

        foreach (var item in orderItems)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            if (product == null)
                continue;

            var itemTotal = item.PriceExclTax * item.Quantity;

            // 1. Resolve commission percentage with hierarchy
            var commissionPercentage = await _commissionService.ResolveCommissionPercentageAsync(product);

            decimal grossProfit;
            if (commissionPercentage.HasValue)
            {
                // Gross Profit = ItemTotal * Commission %
                grossProfit = (commissionPercentage.Value / 100m) * itemTotal;
            }
            else
            {
                // Fallback to Product Cost: Price - ProductCost
                var totalCost = product.ProductCost * item.Quantity;
                grossProfit = Math.Max(0m, itemTotal - totalCost);
            }

            // 2. Regular item discount
            var regularItemDiscount = item.DiscountAmountExclTax;

            // 3. Proportional overall order discount (e.g. group purchase discount or cart coupons)
            var proportionalOrderDiscount = 0m;
            if (order.OrderDiscount > 0)
            {
                proportionalOrderDiscount = order.OrderDiscount * (itemTotal / orderSubtotal);
            }

            // 4. Net Profit = Gross Profit - Regular Discount - Group Purchase Discount
            var netItemProfit = Math.Max(0m, grossProfit - regularItemDiscount - proportionalOrderDiscount);
            totalNetProfit += netItemProfit;
        }

        return totalNetProfit;
    }

    #endregion
}
