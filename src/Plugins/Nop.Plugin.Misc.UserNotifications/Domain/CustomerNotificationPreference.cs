using Nop.Core;

namespace Nop.Plugin.Misc.UserNotifications.Domain;

/// <summary>
/// Represents customer notification settings & preferences
/// </summary>
public partial class CustomerNotificationPreference : BaseEntity
{
    public int CustomerId { get; set; }
    public bool EmailEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; } = true;
    public bool OnSiteToastsEnabled { get; set; } = true;
    public bool SoundEnabled { get; set; } = true;
    public bool OrderUpdatesEnabled { get; set; } = true;
    public bool PromotionsEnabled { get; set; } = true;
    public bool SystemAnnouncementsEnabled { get; set; } = true;
    public DateTime UpdatedOnUtc { get; set; } = DateTime.UtcNow;
}
