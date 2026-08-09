using Nop.Plugin.Misc.UserNotifications.Domain;

namespace Nop.Plugin.Misc.UserNotifications.Services;

/// <summary>
/// Service for customer account inbox notifications
/// </summary>
public interface IUserInboxService
{
    Task AddInboxMessageAsync(int customerId, string title, string message, string actionUrl = null);
    Task<IList<CustomerInboxMessage>> GetCustomerInboxAsync(int customerId, int pageIndex = 0, int pageSize = 20);
    Task<int> GetUnreadCountAsync(int customerId);
    Task MarkAsReadAsync(int messageId, int customerId);
    Task MarkAllAsReadAsync(int customerId);
}
