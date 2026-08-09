using Nop.Plugin.Misc.UserNotifications.Domain;

namespace Nop.Plugin.Misc.UserNotifications.Services;

/// <summary>
/// Service for managing storefront popups and toast modals
/// </summary>
public interface IPopupNotificationService
{
    Task AddPopupAsync(int customerId, string title, string message, string actionUrl = null, string popupType = "Modal");
    Task<IList<CustomerPendingPopup>> GetActivePopupsForCustomerAsync(int customerId);
    Task DismissPopupAsync(int popupId, int customerId);
}
