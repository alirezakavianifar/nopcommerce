using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Services;

/// <summary>
/// Reward rule service
/// </summary>
public class RewardRuleService : IRewardRuleService
{
    #region Fields

    private readonly IRepository<RewardRule> _rewardRuleRepository;

    #endregion

    #region Ctor

    public RewardRuleService(IRepository<RewardRule> rewardRuleRepository)
    {
        _rewardRuleRepository = rewardRuleRepository;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets a reward rule
    /// </summary>
    /// <param name="rewardRuleId">Reward rule identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the reward rule
    /// </returns>
    public virtual async Task<RewardRule> GetRewardRuleByIdAsync(int rewardRuleId)
    {
        if (rewardRuleId == 0)
            return null;

        return await _rewardRuleRepository.GetByIdAsync(rewardRuleId);
    }

    /// <summary>
    /// Gets all reward rules
    /// </summary>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the reward rules
    /// </returns>
    public virtual async Task<IPagedList<RewardRule>> GetAllRewardRulesAsync(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _rewardRuleRepository.GetAllPagedAsync(query =>
        {
            return query.OrderBy(rr => rr.Id);
        }, pageIndex, pageSize);
    }

    /// <summary>
    /// Inserts a reward rule
    /// </summary>
    /// <param name="rewardRule">Reward rule</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertRewardRuleAsync(RewardRule rewardRule)
    {
        if (rewardRule == null)
            throw new ArgumentNullException(nameof(rewardRule));

        await _rewardRuleRepository.InsertAsync(rewardRule);
    }

    /// <summary>
    /// Updates the reward rule
    /// </summary>
    /// <param name="rewardRule">Reward rule</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task UpdateRewardRuleAsync(RewardRule rewardRule)
    {
        if (rewardRule == null)
            throw new ArgumentNullException(nameof(rewardRule));

        await _rewardRuleRepository.UpdateAsync(rewardRule);
    }

    /// <summary>
    /// Deletes a reward rule
    /// </summary>
    /// <param name="rewardRule">Reward rule</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task DeleteRewardRuleAsync(RewardRule rewardRule)
    {
        if (rewardRule == null)
            throw new ArgumentNullException(nameof(rewardRule));

        await _rewardRuleRepository.DeleteAsync(rewardRule);
    }

    #endregion
}
