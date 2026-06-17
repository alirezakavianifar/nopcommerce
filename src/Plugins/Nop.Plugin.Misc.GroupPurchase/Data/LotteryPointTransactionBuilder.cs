using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Data;

/// <summary>
/// Represents a lottery point transaction mapping entity
/// </summary>
public class LotteryPointTransactionBuilder : NopEntityBuilder<LotteryPointTransaction>
{
    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        // No special decimal mappings needed here
    }
}
