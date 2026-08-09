namespace Nop.Plugin.Misc.UserNotifications.Domain;

/// <summary>
/// Represents notification delivery channels
/// </summary>
[Flags]
public enum NotificationDeliveryChannel
{
    None = 0,
    Email = 1,
    Sms = 2,
    PopUp = 4,
    Inbox = 8
}
