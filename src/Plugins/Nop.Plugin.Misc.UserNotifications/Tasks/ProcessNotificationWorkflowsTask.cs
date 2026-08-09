using Nop.Plugin.Misc.UserNotifications.Services;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.UserNotifications.Tasks;

public class ProcessNotificationWorkflowsTask : IScheduleTask
{
    private readonly IWorkflowEngineService _workflowEngineService;

    public ProcessNotificationWorkflowsTask(IWorkflowEngineService workflowEngineService)
    {
        _workflowEngineService = workflowEngineService;
    }

    public async Task ExecuteAsync()
    {
        await _workflowEngineService.ProcessPendingQueueItemsAsync();
    }
}
