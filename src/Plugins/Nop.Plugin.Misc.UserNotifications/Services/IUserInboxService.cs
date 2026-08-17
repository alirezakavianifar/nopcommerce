using Nop.Core;
using Nop.Plugin.Misc.UserNotifications.Domain;

namespace Nop.Plugin.Misc.UserNotifications.Services;

/// <summary>
/// Service for customer account inbox notifications
/// </summary>
public interface IUserInboxService
{
    Task AddInboxMessageAsync(
        int customerId,
        string title,
        string message,
        string actionUrl = null,
        string category = "System",
        string icon = null,
        string imageUrl = null,
        string couponCode = null,
        DateTime? expiresOnUtc = null);

    Task<IPagedList<CustomerInboxMessage>> GetCustomerInboxAsync(
        int customerId,
        string category = null,
        bool? unreadOnly = null,
        string searchKeyword = null,
        int pageIndex = 0,
        int pageSize = 20);

    Task<IList<CustomerInboxMessage>> GetRecentCustomerInboxAsync(int customerId, int count = 10);

    Task<int> GetUnreadCountAsync(int customerId);

    Task MarkAsReadAsync(int messageId, int customerId);

    Task MarkAllAsReadAsync(int customerId);

    Task DeleteMessageAsync(int messageId, int customerId);

    Task ClearReadMessagesAsync(int customerId);
}
