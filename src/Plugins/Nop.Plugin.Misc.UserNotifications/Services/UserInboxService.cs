using Nop.Core;
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

    public async Task AddInboxMessageAsync(
        int customerId,
        string title,
        string message,
        string actionUrl = null,
        string category = "System",
        string icon = null,
        string imageUrl = null,
        string couponCode = null,
        DateTime? expiresOnUtc = null)
    {
        var inboxItem = new CustomerInboxMessage
        {
            CustomerId = customerId,
            Title = title,
            Message = message,
            ActionUrl = actionUrl,
            Category = string.IsNullOrWhiteSpace(category) ? "System" : category,
            Icon = icon,
            ImageUrl = imageUrl,
            CouponCode = couponCode,
            ExpiresOnUtc = expiresOnUtc,
            IsRead = false,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _inboxRepository.InsertAsync(inboxItem);
    }

    public async Task<IPagedList<CustomerInboxMessage>> GetCustomerInboxAsync(
        int customerId,
        string category = null,
        bool? unreadOnly = null,
        string searchKeyword = null,
        int pageIndex = 0,
        int pageSize = 20)
    {
        return await _inboxRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(msg => msg.CustomerId == customerId);

            if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(msg => msg.Category == category);
            }

            if (unreadOnly.HasValue && unreadOnly.Value)
            {
                query = query.Where(msg => !msg.IsRead);
            }

            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                var term = searchKeyword.Trim();
                query = query.Where(msg => (msg.Title != null && msg.Title.Contains(term)) ||
                                           (msg.Message != null && msg.Message.Contains(term)) ||
                                           (msg.CouponCode != null && msg.CouponCode.Contains(term)));
            }

            return query.OrderByDescending(msg => msg.CreatedOnUtc);
        }, pageIndex, pageSize);
    }

    public async Task<IList<CustomerInboxMessage>> GetRecentCustomerInboxAsync(int customerId, int count = 10)
    {
        return await _inboxRepository.GetAllAsync(query =>
        {
            return query.Where(msg => msg.CustomerId == customerId)
                        .OrderByDescending(msg => msg.CreatedOnUtc)
                        .Take(count);
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

    public async Task DeleteMessageAsync(int messageId, int customerId)
    {
        var msg = await _inboxRepository.GetByIdAsync(messageId);
        if (msg != null && msg.CustomerId == customerId)
        {
            await _inboxRepository.DeleteAsync(msg);
        }
    }

    public async Task ClearReadMessagesAsync(int customerId)
    {
        var readMessages = await _inboxRepository.GetAllAsync(query =>
        {
            return query.Where(msg => msg.CustomerId == customerId && msg.IsRead);
        });

        await _inboxRepository.DeleteAsync(readMessages);
    }
}
