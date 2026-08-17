using Nop.Data;
using Nop.Plugin.Misc.UserNotifications.Domain;

namespace Nop.Plugin.Misc.UserNotifications.Services;

public class PopupNotificationService : IPopupNotificationService
{
    private readonly IRepository<CustomerPendingPopup> _popupRepository;

    public PopupNotificationService(IRepository<CustomerPendingPopup> popupRepository)
    {
        _popupRepository = popupRepository;
    }

    public async Task AddPopupAsync(
        int customerId,
        string title,
        string message,
        string actionUrl = null,
        string popupType = "Toast",
        string category = "Promotion",
        string icon = null,
        string imageUrl = null,
        string couponCode = null,
        DateTime? expiresOnUtc = null)
    {
        var popup = new CustomerPendingPopup
        {
            CustomerId = customerId,
            Title = title,
            Message = message,
            ActionUrl = actionUrl,
            PopupType = string.IsNullOrWhiteSpace(popupType) ? "Toast" : popupType,
            Category = string.IsNullOrWhiteSpace(category) ? "Promotion" : category,
            Icon = icon,
            ImageUrl = imageUrl,
            CouponCode = couponCode,
            ExpiresOnUtc = expiresOnUtc,
            IsDismissed = false,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _popupRepository.InsertAsync(popup);
    }

    public async Task<IList<CustomerPendingPopup>> GetActivePopupsForCustomerAsync(int customerId)
    {
        return await _popupRepository.GetAllAsync(query =>
        {
            return from popup in query
                   where popup.CustomerId == customerId && !popup.IsDismissed
                   orderby popup.CreatedOnUtc descending
                   select popup;
        });
    }

    public async Task DismissPopupAsync(int popupId, int customerId)
    {
        var popup = await _popupRepository.GetByIdAsync(popupId);
        if (popup != null && popup.CustomerId == customerId)
        {
            popup.IsDismissed = true;
            await _popupRepository.UpdateAsync(popup);
        }
    }

    public async Task DismissAllPopupsAsync(int customerId)
    {
        var popups = await _popupRepository.GetAllAsync(query =>
        {
            return query.Where(p => p.CustomerId == customerId && !p.IsDismissed);
        });

        foreach (var popup in popups)
        {
            popup.IsDismissed = true;
            await _popupRepository.UpdateAsync(popup);
        }
    }
}
