using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Plugin.Misc.GroupPurchase.Domain;
using Nop.Services.Common;

namespace Nop.Plugin.Misc.GroupPurchase.Services;

/// <summary>
/// Group purchase service
/// </summary>
public partial class GroupPurchaseService : IGroupPurchaseService
{
    protected readonly IRepository<Domain.GroupPurchase> _groupPurchaseRepository;
    protected readonly IRepository<GroupPurchaseMember> _groupPurchaseMemberRepository;
    protected readonly IRepository<LegalConfirmationLog> _legalConfirmationLogRepository;
    protected readonly IGenericAttributeService _genericAttributeService;

    public GroupPurchaseService(
        IRepository<Domain.GroupPurchase> groupPurchaseRepository,
        IRepository<GroupPurchaseMember> groupPurchaseMemberRepository,
        IRepository<LegalConfirmationLog> legalConfirmationLogRepository,
        IGenericAttributeService genericAttributeService)
    {
        _groupPurchaseRepository = groupPurchaseRepository;
        _groupPurchaseMemberRepository = groupPurchaseMemberRepository;
        _legalConfirmationLogRepository = legalConfirmationLogRepository;
        _genericAttributeService = genericAttributeService;
    }

    /// <summary>
    /// Creates a group purchase
    /// </summary>
    /// <param name="leader">Leader customer</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the group purchase</returns>
    public virtual async Task<Domain.GroupPurchase> CreateGroupPurchaseAsync(Customer leader)
    {
        if (leader == null)
            throw new ArgumentNullException(nameof(leader));

        var groupPurchase = new Domain.GroupPurchase
        {
            LeaderCustomerId = leader.Id,
            UniqueCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant(),
            Status = GroupPurchaseStatus.Active,
            CreatedOnUtc = DateTime.UtcNow
            // In a real scenario, we would populate DeliveryCity from leader's address here
        };

        await _groupPurchaseRepository.InsertAsync(groupPurchase);

        // Add leader as a member
        var member = new GroupPurchaseMember
        {
            GroupPurchaseId = groupPurchase.Id,
            CustomerId = leader.Id,
            IsLeader = true,
            AcceptedTerms = true,
            AcceptedOnUtc = DateTime.UtcNow,
            VisibilityType = VisibilityType.Full
        };

        await _groupPurchaseMemberRepository.InsertAsync(member);

        // Tag the leader's session/customer with the group purchase ID using generic attributes
        await _genericAttributeService.SaveAttributeAsync(leader, "GroupPurchaseId", groupPurchase.Id);

        return groupPurchase;
    }

    /// <summary>
    /// Joins a group purchase
    /// </summary>
    /// <param name="member">Member customer</param>
    /// <param name="uniqueCode">Unique code</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the group purchase member</returns>
    public virtual async Task<GroupPurchaseMember> JoinGroupPurchaseAsync(Customer member, string uniqueCode)
    {
        if (member == null)
            throw new ArgumentNullException(nameof(member));

        var groupPurchase = await GetGroupPurchaseByCodeAsync(uniqueCode);
        if (groupPurchase == null || groupPurchase.Status != GroupPurchaseStatus.Active)
            return null;

        // City validation logic (simplified for Phase 2)
        // if (!string.IsNullOrEmpty(groupPurchase.DeliveryCity)) {
        //    // check member city
        // }

        // Check if already a member
        var existingMember = await _groupPurchaseMemberRepository.Table
            .FirstOrDefaultAsync(m => m.GroupPurchaseId == groupPurchase.Id && m.CustomerId == member.Id);

        if (existingMember != null)
            return existingMember;

        var groupMember = new GroupPurchaseMember
        {
            GroupPurchaseId = groupPurchase.Id,
            CustomerId = member.Id,
            IsLeader = false,
            AcceptedTerms = true,
            AcceptedOnUtc = DateTime.UtcNow,
            VisibilityType = VisibilityType.Full // Default for now
        };

        await _groupPurchaseMemberRepository.InsertAsync(groupMember);

        // Tag the member's session/customer with the group purchase ID
        await _genericAttributeService.SaveAttributeAsync(member, "GroupPurchaseId", groupPurchase.Id);

        return groupMember;
    }

    /// <summary>
    /// Logs a legal confirmation
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="groupPurchaseId">Group purchase identifier</param>
    /// <param name="confirmationType">Confirmation type</param>
    /// <param name="ipAddress">IP address</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task LogLegalConfirmationAsync(int customerId, int groupPurchaseId, string confirmationType, string ipAddress)
    {
        var log = new LegalConfirmationLog
        {
            CustomerId = customerId,
            GroupPurchaseId = groupPurchaseId,
            ConfirmationType = confirmationType,
            IpAddress = ipAddress,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _legalConfirmationLogRepository.InsertAsync(log);
    }

    /// <summary>
    /// Gets all group purchases
    /// </summary>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the paged list of group purchases</returns>
    public virtual async Task<IPagedList<Domain.GroupPurchase>> GetAllGroupPurchasesAsync(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = _groupPurchaseRepository.Table.OrderByDescending(gp => gp.CreatedOnUtc);
        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// Gets a group purchase by unique code
    /// </summary>
    /// <param name="uniqueCode">Unique code</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the group purchase</returns>
    public virtual async Task<Domain.GroupPurchase> GetGroupPurchaseByCodeAsync(string uniqueCode)
    {
        if (string.IsNullOrWhiteSpace(uniqueCode))
            return null;

        return await _groupPurchaseRepository.Table
            .FirstOrDefaultAsync(gp => gp.UniqueCode == uniqueCode);
    }

    /// <summary>
    /// Gets a group purchase by identifier
    /// </summary>
    /// <param name="groupPurchaseId">Group purchase identifier</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the group purchase</returns>
    public virtual async Task<Domain.GroupPurchase> GetGroupPurchaseByIdAsync(int groupPurchaseId)
    {
        if (groupPurchaseId == 0)
            return null;

        return await _groupPurchaseRepository.GetByIdAsync(groupPurchaseId);
    }

    /// <summary>
    /// Gets group purchases created by the customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of group purchases</returns>
    public virtual async Task<IList<Domain.GroupPurchase>> GetLeaderGroupsAsync(int customerId)
    {
        if (customerId == 0)
            return new List<Domain.GroupPurchase>();

        var query = _groupPurchaseRepository.Table
            .Where(gp => gp.LeaderCustomerId == customerId)
            .OrderByDescending(gp => gp.CreatedOnUtc);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets group purchase history for a customer as a subgroup member
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of group purchase members</returns>
    public virtual async Task<IList<GroupPurchaseMember>> GetSubgroupHistoryAsync(int customerId)
    {
        if (customerId == 0)
            return new List<GroupPurchaseMember>();

        var query = _groupPurchaseMemberRepository.Table
            .Where(m => m.CustomerId == customerId && !m.IsLeader)
            .OrderByDescending(m => m.AcceptedOnUtc);

        return await query.ToListAsync();
    }
}
