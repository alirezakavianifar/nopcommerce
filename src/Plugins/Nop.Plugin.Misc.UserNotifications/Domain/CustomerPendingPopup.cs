using Nop.Core;

namespace Nop.Plugin.Misc.UserNotifications.Domain;

/// <summary>
/// Represents a pending storefront popup message for a customer
/// </summary>
public partial class CustomerPendingPopup : BaseEntity
{
    public int CustomerId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string ActionUrl { get; set; }
    public string PopupType { get; set; } // Toast, Modal, Celebration, Banner
    public string Category { get; set; }
    public string Icon { get; set; }
    public string ImageUrl { get; set; }
    public string CouponCode { get; set; }
    public DateTime? ExpiresOnUtc { get; set; }
    public bool IsDismissed { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
