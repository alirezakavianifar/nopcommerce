using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Shipping.ConditionalMethods.Domain;

namespace Nop.Plugin.Shipping.ConditionalMethods.Data;

[NopMigration("2026/04/28 10:00:00:0000000", "ConditionalMethods base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<ShippingCityMapping>();
        Create.TableFor<ShippingProductMapping>();
        Create.TableFor<ShippingWarehouseMapping>();
    }
}
