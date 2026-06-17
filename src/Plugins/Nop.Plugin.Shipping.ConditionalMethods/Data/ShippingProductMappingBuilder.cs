using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Shipping.ConditionalMethods.Domain;

namespace Nop.Plugin.Shipping.ConditionalMethods.Data;

public class ShippingProductMappingBuilder : NopEntityBuilder<ShippingProductMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ShippingProductMapping.ShippingTypeId)).AsInt32().NotNullable()
            .WithColumn(nameof(ShippingProductMapping.ProductId)).AsInt32().NotNullable();
    }
}
