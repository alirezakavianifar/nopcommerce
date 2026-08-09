using Nop.Core;

namespace Nop.Plugin.Misc.UserNotifications.Domain;

/// <summary>
/// Represents a notification workflow
/// </summary>
public partial class NotificationWorkflow : BaseEntity
{
    /// <summary>
    /// Gets or sets the workflow name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the trigger type ID
    /// </summary>
    public int TriggerTypeId { get; set; }

    /// <summary>
    /// Gets or sets the trigger type
    /// </summary>
    public NotificationTriggerType TriggerType
    {
        get => (NotificationTriggerType)TriggerTypeId;
        set => TriggerTypeId = (int)value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the workflow is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the creation date
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }
}
