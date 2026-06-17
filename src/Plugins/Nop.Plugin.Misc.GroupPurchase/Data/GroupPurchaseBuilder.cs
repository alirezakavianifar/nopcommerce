using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Data;

public class GroupPurchaseBuilder : NopEntityBuilder<Domain.GroupPurchase>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(Domain.GroupPurchase.LeaderCustomerId)).AsInt32().NotNullable()
            .WithColumn(nameof(Domain.GroupPurchase.UniqueCode)).AsString(100).NotNullable()
            .WithColumn(nameof(Domain.GroupPurchase.StatusId)).AsInt32().NotNullable()
            .WithColumn(nameof(Domain.GroupPurchase.CreatedOnUtc)).AsDateTime().NotNullable()
            .WithColumn(nameof(Domain.GroupPurchase.DeliveryCity)).AsString(400).Nullable()
            .WithColumn(nameof(Domain.GroupPurchase.DeliveryAddress)).AsString(int.MaxValue).Nullable();
    }
}
