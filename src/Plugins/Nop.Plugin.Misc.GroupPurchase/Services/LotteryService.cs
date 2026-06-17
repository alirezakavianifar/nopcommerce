using Nop.Data;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Services;

/// <summary>
/// Lottery service
/// </summary>
public class LotteryService : ILotteryService
{
    #region Fields

    private readonly IRepository<LotteryPointTransaction> _lotteryPointTransactionRepository;

    #endregion

    #region Ctor

    public LotteryService(IRepository<LotteryPointTransaction> lotteryPointTransactionRepository)
    {
        _lotteryPointTransactionRepository = lotteryPointTransactionRepository;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Adds lottery points
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="points">Points to add (can be negative)</param>
    /// <param name="source">Source of points</param>
    /// <param name="groupPurchaseId">Related group purchase (optional)</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task AddPointsAsync(int customerId, int points, LotterySource source, int? groupPurchaseId = null)
    {
        if (points == 0)
            return;

        var transaction = new LotteryPointTransaction
        {
            CustomerId = customerId,
            Points = points,
            SourceId = (int)source,
            GroupPurchaseId = groupPurchaseId,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _lotteryPointTransactionRepository.InsertAsync(transaction);
    }

    /// <summary>
    /// Gets total lottery points for a customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<int> GetTotalPointsAsync(int customerId)
    {
        var transactions = await _lotteryPointTransactionRepository.GetAllAsync(query =>
            query.Where(lpt => lpt.CustomerId == customerId)
        );

        return transactions.Sum(lpt => lpt.Points);
    }

    #endregion
}
