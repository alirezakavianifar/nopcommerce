using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Data;

[NopMigration("2026/02/19 14:00:00:0000000", "GroupPurchase base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<Domain.GroupPurchase>();
        Create.TableFor<GroupPurchaseMember>();
        Create.TableFor<LegalConfirmationLog>();
        Create.TableFor<RewardRule>();
        Create.TableFor<GroupPurchaseReward>();
        Create.TableFor<CustomerWallet>();
        Create.TableFor<WalletTransaction>();
        Create.TableFor<LotteryPointTransaction>();
    }
}
