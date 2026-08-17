using Nop.Plugin.Misc.UserNotifications.Domain;

namespace Nop.Plugin.Misc.UserNotifications.Services;

/// <summary>
/// Service for managing customer notification preferences
/// </summary>
public interface INotificationPreferenceService
{
    Task<CustomerNotificationPreference> GetCustomerPreferencesAsync(int customerId);
    Task SaveCustomerPreferencesAsync(CustomerNotificationPreference preferences);
    Task<bool> IsNotificationAllowedAsync(int customerId, string channel, string category);
}
