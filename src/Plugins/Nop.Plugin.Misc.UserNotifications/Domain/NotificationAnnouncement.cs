using Nop.Core;

namespace Nop.Plugin.Misc.UserNotifications.Domain;

/// <summary>
/// Represents a notification announcement
/// </summary>
public partial class NotificationAnnouncement : BaseEntity
{
    /// <summary>
    /// Gets or sets the title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the body
    /// </summary>
    public string Body { get; set; }

    /// <summary>
    /// Gets or sets the start date and time in UTC
    /// </summary>
    public DateTime? StartDateUtc { get; set; }

    /// <summary>
    /// Gets or sets the end date and time in UTC
    /// </summary>
    public DateTime? EndDateUtc { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the announcement is published
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Gets or sets the date and time of instance creation
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }
}
