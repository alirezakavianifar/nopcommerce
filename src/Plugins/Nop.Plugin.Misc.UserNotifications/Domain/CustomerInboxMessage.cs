using Nop.Core;

namespace Nop.Plugin.Misc.UserNotifications.Domain;

/// <summary>
/// Represents a customer account inbox message
/// </summary>
public partial class CustomerInboxMessage : BaseEntity
{
    public int CustomerId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string ActionUrl { get; set; }
    public string Category { get; set; } // Order, Promotion, System, Product, Security
    public string Icon { get; set; }
    public string ImageUrl { get; set; }
    public string CouponCode { get; set; }
    public DateTime? ExpiresOnUtc { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
