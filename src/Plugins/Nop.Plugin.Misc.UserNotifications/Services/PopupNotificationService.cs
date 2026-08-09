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

    public async Task AddPopupAsync(int customerId, string title, string message, string actionUrl = null, string popupType = "Modal")
    {
        var popup = new CustomerPendingPopup
        {
            CustomerId = customerId,
            Title = title,
            Message = message,
            ActionUrl = actionUrl,
            PopupType = popupType,
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
}
