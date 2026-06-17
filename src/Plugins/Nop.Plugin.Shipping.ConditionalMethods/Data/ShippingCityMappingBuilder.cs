using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Shipping.ConditionalMethods.Domain;

namespace Nop.Plugin.Shipping.ConditionalMethods.Data;

public class ShippingCityMappingBuilder : NopEntityBuilder<ShippingCityMapping>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ShippingCityMapping.ShippingTypeId)).AsInt32().NotNullable()
            .WithColumn(nameof(ShippingCityMapping.CityName)).AsString(256).NotNullable()
            .WithColumn(nameof(ShippingCityMapping.StateProvinceId)).AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn(nameof(ShippingCityMapping.IsActive)).AsBoolean().NotNullable().WithDefaultValue(true);
    }
}
