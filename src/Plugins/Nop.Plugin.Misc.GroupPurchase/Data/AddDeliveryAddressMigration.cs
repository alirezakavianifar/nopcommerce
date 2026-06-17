using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.GroupPurchase.Data;

[NopMigration("2026/02/21 09:50:00:0000000", "GroupPurchase add DeliveryAddress column", MigrationProcessType.Update)]
public class AddDeliveryAddressMigration : Migration
{
    public override void Up()
    {
        // Add DeliveryAddress column if it doesn't already exist
        if (Schema.Table("GroupPurchase").Exists() &&
            !Schema.Table("GroupPurchase").Column("DeliveryAddress").Exists())
        {
            Alter.Table("GroupPurchase")
                .AddColumn("DeliveryAddress").AsString(int.MaxValue).Nullable();
        }
    }

    public override void Down()
    {
        if (Schema.Table("GroupPurchase").Column("DeliveryAddress").Exists())
        {
            Delete.Column("DeliveryAddress").FromTable("GroupPurchase");
        }
    }
}
