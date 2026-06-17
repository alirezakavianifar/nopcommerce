using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Data;

/// <summary>
/// Represents a reward rule mapping entity
/// </summary>
public class RewardRuleBuilder : NopEntityBuilder<RewardRule>
{
    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(RewardRule.Value)).AsDecimal(18, 4)
            .WithColumn(nameof(RewardRule.MinCartAmount)).AsDecimal(18, 4);
    }
}
