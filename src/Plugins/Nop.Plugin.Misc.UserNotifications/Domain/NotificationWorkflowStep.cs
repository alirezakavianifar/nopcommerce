using Nop.Core;

namespace Nop.Plugin.Misc.UserNotifications.Domain;

/// <summary>
/// Represents a step within a notification workflow
/// </summary>
public partial class NotificationWorkflowStep : BaseEntity
{
    /// <summary>
    /// Gets or sets the parent workflow ID
    /// </summary>
    public int WorkflowId { get; set; }

    /// <summary>
    /// Gets or sets the step order sequence
    /// </summary>
    public int StepOrder { get; set; }

    /// <summary>
    /// Gets or sets delay in minutes before firing (0 = immediate)
    /// </summary>
    public int DelayMinutes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to send via Email
    /// </summary>
    public bool SendEmail { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to send via SMS
    /// </summary>
    public bool SendSms { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to send via Storefront PopUp
    /// </summary>
    public bool SendPopUp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to send via Account Inbox
    /// </summary>
    public bool SendInbox { get; set; }

    /// <summary>
    /// Gets or sets the notification title/subject template
    /// </summary>
    public string SubjectTemplate { get; set; }

    /// <summary>
    /// Gets or sets the notification body template
    /// </summary>
    public string BodyTemplate { get; set; }

    /// <summary>
    /// Gets or sets FarazSMS Pattern Code (optional)
    /// </summary>
    public string SmsPatternCode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to auto-generate single-use coupon discount
    /// </summary>
    public bool GenerateDiscountCode { get; set; }

    /// <summary>
    /// Gets or sets discount amount percentage (e.g. 10.0 for 10% off)
    /// </summary>
    public decimal DiscountPercentage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether step is active
    /// </summary>
    public bool IsActive { get; set; }
}
