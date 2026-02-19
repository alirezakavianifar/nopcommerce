using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.AmazingDiscounts.Domain;

namespace Nop.Plugin.Misc.AmazingDiscounts.Data;

[NopMigration("2026/02/19 13:00:00:0000000", "AmazingDiscounts base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<AmazingDiscountProduct>();
    }
}
