using Nop.Core;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Services;

/// <summary>
/// Reward rule service interface
/// </summary>
public interface IRewardRuleService
{
    /// <summary>
    /// Gets a reward rule
    /// </summary>
    /// <param name="rewardRuleId">Reward rule identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the reward rule
    /// </returns>
    Task<RewardRule> GetRewardRuleByIdAsync(int rewardRuleId);

    /// <summary>
    /// Gets all reward rules
    /// </summary>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the reward rules
    /// </returns>
    Task<IPagedList<RewardRule>> GetAllRewardRulesAsync(int pageIndex = 0, int pageSize = int.MaxValue);

    /// <summary>
    /// Inserts a reward rule
    /// </summary>
    /// <param name="rewardRule">Reward rule</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertRewardRuleAsync(RewardRule rewardRule);

    /// <summary>
    /// Updates the reward rule
    /// </summary>
    /// <param name="rewardRule">Reward rule</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateRewardRuleAsync(RewardRule rewardRule);

    /// <summary>
    /// Deletes a reward rule
    /// </summary>
    /// <param name="rewardRule">Reward rule</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteRewardRuleAsync(RewardRule rewardRule);
}
