using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Data;

/// <summary>
/// Represents a customer wallet mapping entity
/// </summary>
public class CustomerWalletBuilder : NopEntityBuilder<CustomerWallet>
{
    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(CustomerWallet.Balance)).AsDecimal(18, 4);
    }
}
