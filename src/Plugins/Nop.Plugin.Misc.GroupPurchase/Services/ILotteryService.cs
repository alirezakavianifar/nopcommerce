using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Services;

/// <summary>
/// Lottery service interface
/// </summary>
public interface ILotteryService
{
    /// <summary>
    /// Adds lottery points
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="points">Points to add (can be negative)</param>
    /// <param name="source">Source of points</param>
    /// <param name="groupPurchaseId">Related group purchase (optional)</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task AddPointsAsync(int customerId, int points, LotterySource source, int? groupPurchaseId = null);

    /// <summary>
    /// Gets total lottery points for a customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<int> GetTotalPointsAsync(int customerId);
}
