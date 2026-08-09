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
    public bool IsRead { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
