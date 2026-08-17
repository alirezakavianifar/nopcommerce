using Nop.Data;
using Nop.Plugin.Misc.UserNotifications.Domain;

namespace Nop.Plugin.Misc.UserNotifications.Services;

public class NotificationPreferenceService : INotificationPreferenceService
{
    private readonly IRepository<CustomerNotificationPreference> _preferenceRepository;

    public NotificationPreferenceService(IRepository<CustomerNotificationPreference> preferenceRepository)
    {
        _preferenceRepository = preferenceRepository;
    }

    public async Task<CustomerNotificationPreference> GetCustomerPreferencesAsync(int customerId)
    {
        var preferences = (await _preferenceRepository.GetAllAsync(query =>
        {
            return query.Where(p => p.CustomerId == customerId);
        })).FirstOrDefault();

        if (preferences == null)
        {
            preferences = new CustomerNotificationPreference
            {
                CustomerId = customerId,
                EmailEnabled = true,
                SmsEnabled = true,
                OnSiteToastsEnabled = true,
                SoundEnabled = true,
                OrderUpdatesEnabled = true,
                PromotionsEnabled = true,
                SystemAnnouncementsEnabled = true,
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _preferenceRepository.InsertAsync(preferences);
        }

        return preferences;
    }

    public async Task SaveCustomerPreferencesAsync(CustomerNotificationPreference preferences)
    {
        if (preferences == null)
            return;

        preferences.UpdatedOnUtc = DateTime.UtcNow;

        if (preferences.Id > 0)
        {
            await _preferenceRepository.UpdateAsync(preferences);
        }
        else
        {
            await _preferenceRepository.InsertAsync(preferences);
        }
    }

    public async Task<bool> IsNotificationAllowedAsync(int customerId, string channel, string category)
    {
        var prefs = await GetCustomerPreferencesAsync(customerId);

        // Check channel
        if (channel.Equals("Email", StringComparison.OrdinalIgnoreCase) && !prefs.EmailEnabled)
            return false;

        if (channel.Equals("Sms", StringComparison.OrdinalIgnoreCase) && !prefs.SmsEnabled)
            return false;

        if ((channel.Equals("Toast", StringComparison.OrdinalIgnoreCase) || channel.Equals("PopUp", StringComparison.OrdinalIgnoreCase)) && !prefs.OnSiteToastsEnabled)
            return false;

        // Check topic category
        if (category.Equals("Order", StringComparison.OrdinalIgnoreCase) && !prefs.OrderUpdatesEnabled)
            return false;

        if (category.Equals("Promotion", StringComparison.OrdinalIgnoreCase) && !prefs.PromotionsEnabled)
            return false;

        if (category.Equals("System", StringComparison.OrdinalIgnoreCase) && !prefs.SystemAnnouncementsEnabled)
            return false;

        return true;
    }
}
