using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.ArtificialIntelligence.Domain;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Data;

public class ProductEmbeddingCacheBuilder : NopEntityBuilder<ProductEmbeddingCache>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ProductEmbeddingCache.ProductId)).AsInt32().NotNullable()
            .WithColumn(nameof(ProductEmbeddingCache.VectorJson)).AsString(int.MaxValue).NotNullable()
            .WithColumn(nameof(ProductEmbeddingCache.LastUpdatedOnUtc)).AsDateTime().NotNullable();
    }
}
