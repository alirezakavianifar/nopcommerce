using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.ArtificialIntelligence.Domain;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Data;

public class AiDuplicateProductQueueBuilder : NopEntityBuilder<AiDuplicateProductQueue>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(AiDuplicateProductQueue.ProductId)).AsInt32().NotNullable()
            .WithColumn(nameof(AiDuplicateProductQueue.VendorId)).AsInt32().NotNullable()
            .WithColumn(nameof(AiDuplicateProductQueue.DuplicateProductId)).AsInt32().NotNullable()
            .WithColumn(nameof(AiDuplicateProductQueue.StatusId)).AsInt32().NotNullable()
            .WithColumn(nameof(AiDuplicateProductQueue.Explanation)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(AiDuplicateProductQueue.CreatedOnUtc)).AsDateTime().NotNullable()
            .WithColumn(nameof(AiDuplicateProductQueue.UpdatedOnUtc)).AsDateTime().Nullable();
    }
}
