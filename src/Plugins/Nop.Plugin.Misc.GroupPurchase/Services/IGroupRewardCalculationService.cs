using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Services;

/// <summary>
/// Group reward calculation service interface
/// </summary>
public interface IGroupRewardCalculationService
{
    /// <summary>
    /// Calculate and apply reward for a group purchase order placement
    /// </summary>
    /// <param name="order">The order</param>
    /// <param name="groupPurchase">The group purchase</param>
    /// <param name="member">The member</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task CalculateAndApplyRewardAsync(Order order, Domain.GroupPurchase groupPurchase, GroupPurchaseMember member);
}
