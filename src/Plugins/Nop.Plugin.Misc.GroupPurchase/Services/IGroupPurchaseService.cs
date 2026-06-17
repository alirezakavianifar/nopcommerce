using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Services;

/// <summary>
/// Group purchase service interface
/// </summary>
public partial interface IGroupPurchaseService
{
    /// <summary>
    /// Creates a group purchase
    /// </summary>
    /// <param name="leader">Leader customer</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the group purchase</returns>
    Task<Domain.GroupPurchase> CreateGroupPurchaseAsync(Customer leader);

    /// <summary>
    /// Joins a group purchase
    /// </summary>
    /// <param name="member">Member customer</param>
    /// <param name="uniqueCode">Unique code</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the group purchase member</returns>
    Task<GroupPurchaseMember> JoinGroupPurchaseAsync(Customer member, string uniqueCode);

    /// <summary>
    /// Gets a group purchase by unique code
    /// </summary>
    /// <param name="uniqueCode">Unique code</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the group purchase</returns>
    Task<Domain.GroupPurchase> GetGroupPurchaseByCodeAsync(string uniqueCode);

    /// <summary>
    /// Gets all group purchases
    /// </summary>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the paged list of group purchases</returns>
    Task<IPagedList<Domain.GroupPurchase>> GetAllGroupPurchasesAsync(int pageIndex = 0, int pageSize = int.MaxValue);

    /// <summary>
    /// Logs a legal confirmation
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="groupPurchaseId">Group purchase identifier</param>
    /// <param name="confirmationType">Confirmation type</param>
    /// <param name="ipAddress">IP address</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task LogLegalConfirmationAsync(int customerId, int groupPurchaseId, string confirmationType, string ipAddress);

    /// <summary>
    /// Gets a group purchase by identifier
    /// </summary>
    /// <param name="groupPurchaseId">Group purchase identifier</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the group purchase</returns>
    Task<Domain.GroupPurchase> GetGroupPurchaseByIdAsync(int groupPurchaseId);

    /// <summary>
    /// Gets group purchases created by the customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of group purchases</returns>
    Task<IList<Domain.GroupPurchase>> GetLeaderGroupsAsync(int customerId);

    /// <summary>
    /// Gets group purchase history for a customer as a subgroup member
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of group purchase members</returns>
    Task<IList<GroupPurchaseMember>> GetSubgroupHistoryAsync(int customerId);
}
