using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Plugin.Misc.UserNotifications.Domain;
using Nop.Plugin.Misc.UserNotifications.Services;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.UserNotifications.Infrastructure;

public class NotificationEventConsumer :
    IConsumer<CustomerRegisteredEvent>,
    IConsumer<OrderPlacedEvent>,
    IConsumer<EntityInsertedEvent<ShoppingCartItem>>
{
    private readonly IWorkflowEngineService _workflowEngineService;

    public NotificationEventConsumer(IWorkflowEngineService workflowEngineService)
    {
        _workflowEngineService = workflowEngineService;
    }

    public async Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
    {
        if (eventMessage?.Customer != null)
        {
            await _workflowEngineService.TriggerWorkflowAsync(
                NotificationTriggerType.CustomerRegistered,
                eventMessage.Customer.Id);
        }
    }

    public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
    {
        if (eventMessage?.Order != null)
        {
            await _workflowEngineService.TriggerWorkflowAsync(
                NotificationTriggerType.OrderPlaced,
                eventMessage.Order.CustomerId,
                orderId: eventMessage.Order.Id);
        }
    }

    public async Task HandleEventAsync(EntityInsertedEvent<ShoppingCartItem> eventMessage)
    {
        var item = eventMessage?.Entity;
        if (item != null && item.ShoppingCartType == ShoppingCartType.Wishlist)
        {
            await _workflowEngineService.TriggerWorkflowAsync(
                NotificationTriggerType.WishlistAdded,
                item.CustomerId,
                productId: item.ProductId);
        }
    }
}
