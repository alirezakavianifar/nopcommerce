using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Data;

public class LegalConfirmationLogBuilder : NopEntityBuilder<LegalConfirmationLog>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(LegalConfirmationLog.CustomerId)).AsInt32().NotNullable()
            .WithColumn(nameof(LegalConfirmationLog.GroupPurchaseId)).AsInt32().NotNullable()
            .WithColumn(nameof(LegalConfirmationLog.ConfirmationType)).AsString(100).NotNullable()
            .WithColumn(nameof(LegalConfirmationLog.IpAddress)).AsString(100).Nullable()
            .WithColumn(nameof(LegalConfirmationLog.CreatedOnUtc)).AsDateTime().NotNullable();
    }
}
