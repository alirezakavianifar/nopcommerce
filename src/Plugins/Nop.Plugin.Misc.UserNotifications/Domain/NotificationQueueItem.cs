using Nop.Core;

namespace Nop.Plugin.Misc.UserNotifications.Domain;

/// <summary>
/// Represents a queued notification waiting for delay execution or log tracking
/// </summary>
public partial class NotificationQueueItem : BaseEntity
{
    public int WorkflowStepId { get; set; }
    public int CustomerId { get; set; }
    public int? ProductId { get; set; }
    public int? OrderId { get; set; }
    public DateTime ScheduledSendTimeUtc { get; set; }
    public int StatusId { get; set; }

    public NotificationQueueStatus Status
    {
        get => (NotificationQueueStatus)StatusId;
        set => StatusId = (int)value;
    }

    public DateTime? SentOnUtc { get; set; }
    public string RenderedTitle { get; set; }
    public string RenderedBody { get; set; }
    public string GeneratedDiscountCode { get; set; }
    public string DeliveryChannels { get; set; }
    public string ErrorLog { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
