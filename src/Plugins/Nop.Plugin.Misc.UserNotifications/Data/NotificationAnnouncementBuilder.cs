using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.UserNotifications.Domain;

namespace Nop.Plugin.Misc.UserNotifications.Data;

public class NotificationAnnouncementBuilder : NopEntityBuilder<NotificationAnnouncement>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(NotificationAnnouncement.Title)).AsString(400).NotNullable()
            .WithColumn(nameof(NotificationAnnouncement.Body)).AsString(int.MaxValue).NotNullable()
            .WithColumn(nameof(NotificationAnnouncement.StartDateUtc)).AsDateTime().Nullable()
            .WithColumn(nameof(NotificationAnnouncement.EndDateUtc)).AsDateTime().Nullable()
            .WithColumn(nameof(NotificationAnnouncement.IsPublished)).AsBoolean().NotNullable()
            .WithColumn(nameof(NotificationAnnouncement.CreatedOnUtc)).AsDateTime().NotNullable();
    }
}
