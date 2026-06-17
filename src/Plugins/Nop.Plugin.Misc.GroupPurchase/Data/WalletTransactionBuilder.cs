using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Data;

/// <summary>
/// Represents a wallet transaction mapping entity
/// </summary>
public class WalletTransactionBuilder : NopEntityBuilder<WalletTransaction>
{
    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(WalletTransaction.Amount)).AsDecimal(18, 4);
    }
}
