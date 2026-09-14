using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Data;

/// <summary>
/// Represents a group purchase commission mapping entity
/// </summary>
public class GroupPurchaseCommissionBuilder : NopEntityBuilder<GroupPurchaseCommission>
{
    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(GroupPurchaseCommission.CommissionScopeId)).AsInt32().NotNullable()
            .WithColumn(nameof(GroupPurchaseCommission.EntityId)).AsInt32().NotNullable()
            .WithColumn(nameof(GroupPurchaseCommission.Percentage)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(GroupPurchaseCommission.EntityName)).AsString(400).Nullable()
            .WithColumn(nameof(GroupPurchaseCommission.CreatedOnUtc)).AsDateTime().NotNullable()
            .WithColumn(nameof(GroupPurchaseCommission.UpdatedOnUtc)).AsDateTime().NotNullable();
    }
}
