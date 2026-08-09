using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.UserNotifications.Infrastructure;
using Nop.Plugin.Misc.UserNotifications.Services;
using Nop.Plugin.Misc.UserNotifications.Tasks;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.UserNotifications.Infrastructure;

public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserNotificationService, UserNotificationService>();
        services.AddScoped<ISmsNotificationService, FarazSmsNotificationService>();
        services.AddScoped<IUserInboxService, UserInboxService>();
        services.AddScoped<IPopupNotificationService, PopupNotificationService>();
        services.AddScoped<IWorkflowEngineService, WorkflowEngineService>();

        services.AddScoped<IConsumer<CustomerRegisteredEvent>, NotificationEventConsumer>();
        services.AddScoped<IConsumer<OrderPlacedEvent>, NotificationEventConsumer>();
        services.AddScoped<IConsumer<EntityInsertedEvent<ShoppingCartItem>>, NotificationEventConsumer>();

        services.AddScoped<ProcessNotificationWorkflowsTask>();
    }

    public void Configure(IApplicationBuilder application)
    {
        application.UseMiddleware<ProductViewTrackerMiddleware>();
    }

    public int Order => 100;
}
