using System.ComponentModel.DataAnnotations;
using Nop.Plugin.Misc.UserNotifications.Domain;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.UserNotifications.Models;

public record WorkflowModel : BaseNopEntityModel
{
    [Required]
    public string Name { get; set; }
    public int TriggerTypeId { get; set; }
    public string TriggerTypeName => ((NotificationTriggerType)TriggerTypeId).ToString();
    public bool IsActive { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public IList<WorkflowStepModel> Steps { get; set; } = new List<WorkflowStepModel>();
}

public record WorkflowStepModel : BaseNopEntityModel
{
    public int WorkflowId { get; set; }
    public int StepOrder { get; set; }
    public int DelayMinutes { get; set; }
    public bool SendEmail { get; set; }
    public bool SendSms { get; set; }
    public bool SendPopUp { get; set; }
    public bool SendInbox { get; set; }
    public string SubjectTemplate { get; set; }
    public string BodyTemplate { get; set; }
    public string SmsPatternCode { get; set; }
    public bool GenerateDiscountCode { get; set; }
    public decimal DiscountPercentage { get; set; }
    public bool IsActive { get; set; }
}
