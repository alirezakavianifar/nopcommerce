using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Data;

[NopMigration("2026/02/21 10:15:00:0000000", "GroupPurchase recreate all tables if missing", MigrationProcessType.Update)]
public class RecreateTablesMigration : Migration
{
    public override void Up()
    {
        if (!Schema.Table("GroupPurchase").Exists())
            Create.TableFor<Domain.GroupPurchase>();

        if (!Schema.Table("GroupPurchaseMember").Exists())
            Create.TableFor<GroupPurchaseMember>();

        if (!Schema.Table("LegalConfirmationLog").Exists())
            Create.TableFor<LegalConfirmationLog>();

        if (!Schema.Table("RewardRule").Exists())
            Create.TableFor<RewardRule>();

        if (!Schema.Table("GroupPurchaseReward").Exists())
            Create.TableFor<GroupPurchaseReward>();

        if (!Schema.Table("CustomerWallet").Exists())
            Create.TableFor<CustomerWallet>();

        if (!Schema.Table("WalletTransaction").Exists())
            Create.TableFor<WalletTransaction>();

        if (!Schema.Table("LotteryPointTransaction").Exists())
            Create.TableFor<LotteryPointTransaction>();

        // Also add DeliveryAddress column if table existed but column was missing
        if (Schema.Table("GroupPurchase").Exists() &&
            !Schema.Table("GroupPurchase").Column("DeliveryAddress").Exists())
        {
            Alter.Table("GroupPurchase")
                .AddColumn("DeliveryAddress").AsString(int.MaxValue).Nullable();
        }
    }

    public override void Down()
    {
    }
}
