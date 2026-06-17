using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Shipping.ConditionalMethods.Domain;

namespace Nop.Plugin.Shipping.ConditionalMethods.Data;

public class ShippingWarehouseMappingBuilder : NopEntityBuilder<ShippingWarehouseMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ShippingWarehouseMapping.ShippingTypeId)).AsInt32().NotNullable()
            .WithColumn(nameof(ShippingWarehouseMapping.WarehouseId)).AsInt32().NotNullable();
    }
}
