using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Data;
using Nop.Plugin.Misc.GroupPurchase.Domain;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.GroupPurchase.Services;

/// <summary>
/// Event consumer for OrderPlacedEvent
/// </summary>
public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly IGroupRewardCalculationService _groupRewardCalculationService;
    private readonly IRepository<Domain.GroupPurchase> _groupPurchaseRepository;
    private readonly IRepository<GroupPurchaseMember> _groupPurchaseMemberRepository;

    public OrderPlacedEventConsumer(
        IGroupRewardCalculationService groupRewardCalculationService,
        IRepository<Domain.GroupPurchase> groupPurchaseRepository,
        IRepository<GroupPurchaseMember> groupPurchaseMemberRepository)
    {
        _groupRewardCalculationService = groupRewardCalculationService;
        _groupPurchaseRepository = groupPurchaseRepository;
        _groupPurchaseMemberRepository = groupPurchaseMemberRepository;
    }

    /// <summary>
    /// Handle event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
    {
        var order = eventMessage.Order;
        if (order == null)
            return;

        // The group purchase may be linked via custom properties on the order or customer.
        // For phase 2/3, we assume the customer is part of an active group purchase member record.
        var groupPurchaseMember = (await _groupPurchaseMemberRepository.GetAllAsync(query =>
            query.Where(m => m.CustomerId == order.CustomerId && m.AcceptedTerms)
                 .OrderByDescending(m => m.Id)
        )).FirstOrDefault();

        if (groupPurchaseMember == null)
            return;

        var groupPurchase = await _groupPurchaseRepository.GetByIdAsync(groupPurchaseMember.GroupPurchaseId);
        if (groupPurchase == null || groupPurchase.Status != GroupPurchaseStatus.Active)
            return;

        // Calculate and apply rewards
        await _groupRewardCalculationService.CalculateAndApplyRewardAsync(order, groupPurchase, groupPurchaseMember);
    }
}
