using Nop.Data;
using Nop.Plugin.Misc.UserNotifications.Domain;

namespace Nop.Plugin.Misc.UserNotifications.Services;

public class UserInboxService : IUserInboxService
{
    private readonly IRepository<CustomerInboxMessage> _inboxRepository;

    public UserInboxService(IRepository<CustomerInboxMessage> inboxRepository)
    {
        _inboxRepository = inboxRepository;
    }

    public async Task AddInboxMessageAsync(int customerId, string title, string message, string actionUrl = null)
    {
        var inboxItem = new CustomerInboxMessage
        {
            CustomerId = customerId,
            Title = title,
            Message = message,
            ActionUrl = actionUrl,
            IsRead = false,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _inboxRepository.InsertAsync(inboxItem);
    }

    public async Task<IList<CustomerInboxMessage>> GetCustomerInboxAsync(int customerId, int pageIndex = 0, int pageSize = 20)
    {
        return await _inboxRepository.GetAllAsync(query =>
        {
            return query.Where(msg => msg.CustomerId == customerId)
                        .OrderByDescending(msg => msg.CreatedOnUtc);
        });
    }

    public async Task<int> GetUnreadCountAsync(int customerId)
    {
        var unreadMessages = await _inboxRepository.GetAllAsync(query =>
        {
            return query.Where(msg => msg.CustomerId == customerId && !msg.IsRead);
        });
        return unreadMessages.Count;
    }

    public async Task MarkAsReadAsync(int messageId, int customerId)
    {
        var msg = await _inboxRepository.GetByIdAsync(messageId);
        if (msg != null && msg.CustomerId == customerId)
        {
            msg.IsRead = true;
            await _inboxRepository.UpdateAsync(msg);
        }
    }

    public async Task MarkAllAsReadAsync(int customerId)
    {
        var unread = await _inboxRepository.GetAllAsync(query =>
        {
            return query.Where(msg => msg.CustomerId == customerId && !msg.IsRead);
        });

        foreach (var msg in unread)
        {
            msg.IsRead = true;
            await _inboxRepository.UpdateAsync(msg);
        }
    }
}
