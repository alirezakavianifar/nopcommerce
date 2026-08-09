using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.UserNotifications.Domain;

namespace Nop.Plugin.Misc.UserNotifications.Data;

[NopMigration("2026/08/09 12:00:00:0000000", "UserNotifications advanced workflows schema", MigrationProcessType.Installation)]
public class AdvancedSchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<NotificationWorkflow>();
        Create.TableFor<NotificationWorkflowStep>();
        Create.TableFor<NotificationQueueItem>();
        Create.TableFor<CustomerInboxMessage>();
        Create.TableFor<CustomerPendingPopup>();
        Create.TableFor<ProductViewLog>();
    }
}
