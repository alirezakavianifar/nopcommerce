using Nop.Plugin.Misc.UserNotifications.Domain;

namespace Nop.Plugin.Misc.UserNotifications.Services;

/// <summary>
/// Service for triggering, scheduling, and processing automated notification workflows
/// </summary>
public interface IWorkflowEngineService
{
    // Triggering & Queueing
    Task TriggerWorkflowAsync(NotificationTriggerType triggerType, int customerId, int? productId = null, int? orderId = null);
    Task ProcessPendingQueueItemsAsync();

    // Workflow CRUD
    Task<IList<NotificationWorkflow>> GetAllWorkflowsAsync();
    Task<NotificationWorkflow> GetWorkflowByIdAsync(int workflowId);
    Task InsertWorkflowAsync(NotificationWorkflow workflow);
    Task UpdateWorkflowAsync(NotificationWorkflow workflow);
    Task DeleteWorkflowAsync(NotificationWorkflow workflow);

    // Step CRUD
    Task<IList<NotificationWorkflowStep>> GetWorkflowStepsAsync(int workflowId);
    Task<NotificationWorkflowStep> GetStepByIdAsync(int stepId);
    Task InsertStepAsync(NotificationWorkflowStep step);
    Task UpdateStepAsync(NotificationWorkflowStep step);
    Task DeleteStepAsync(NotificationWorkflowStep step);

    // Queue Logs
    Task<IList<NotificationQueueItem>> GetQueueItemsAsync(int pageIndex = 0, int pageSize = 50);
}
