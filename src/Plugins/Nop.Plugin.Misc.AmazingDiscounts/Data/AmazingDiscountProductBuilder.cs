using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.AmazingDiscounts.Domain;

namespace Nop.Plugin.Misc.AmazingDiscounts.Data;

public class AmazingDiscountProductBuilder : NopEntityBuilder<AmazingDiscountProduct>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(AmazingDiscountProduct.ProductId)).AsInt32().NotNullable()
            .WithColumn(nameof(AmazingDiscountProduct.DisplayOrder)).AsInt32().NotNullable()
            .WithColumn(nameof(AmazingDiscountProduct.CustomLabel)).AsString(400).Nullable()
            .WithColumn(nameof(AmazingDiscountProduct.StartDateUtc)).AsDateTime().Nullable()
            .WithColumn(nameof(AmazingDiscountProduct.EndDateUtc)).AsDateTime().Nullable();
    }
}
