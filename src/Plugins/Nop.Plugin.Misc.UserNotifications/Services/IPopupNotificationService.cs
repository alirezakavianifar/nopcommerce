using Nop.Plugin.Misc.UserNotifications.Domain;

namespace Nop.Plugin.Misc.UserNotifications.Services;

/// <summary>
/// Service for managing storefront popups, celebration modals, and floating toasts
/// </summary>
public interface IPopupNotificationService
{
    Task AddPopupAsync(
        int customerId,
        string title,
        string message,
        string actionUrl = null,
        string popupType = "Toast",
        string category = "Promotion",
        string icon = null,
        string imageUrl = null,
        string couponCode = null,
        DateTime? expiresOnUtc = null);

    Task<IList<CustomerPendingPopup>> GetActivePopupsForCustomerAsync(int customerId);

    Task DismissPopupAsync(int popupId, int customerId);

    Task DismissAllPopupsAsync(int customerId);
}
