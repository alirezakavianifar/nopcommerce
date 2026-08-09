namespace Nop.Plugin.Misc.UserNotifications.Domain;

/// <summary>
/// Represents notification queue item status
/// </summary>
public enum NotificationQueueStatus
{
    Pending = 10,
    Processing = 20,
    Sent = 30,
    Failed = 40
}
