using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Data;

[NopMigration("2026/02/21 09:10:00:0000000", "GroupPurchase force table creation", MigrationProcessType.Update)]
public class FixTablesMigration : Migration
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
    }

    public override void Down()
    {
    }
}
