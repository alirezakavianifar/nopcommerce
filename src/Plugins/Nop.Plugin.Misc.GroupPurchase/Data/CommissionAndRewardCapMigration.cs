using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Data;

[NopMigration("2026/09/14 11:30:00:0000000", "GroupPurchase commission hierarchy and reward cap schema", MigrationProcessType.Update)]
public class CommissionAndRewardCapMigration : AutoReversingMigration
{
    public override void Up()
    {
        if (!Schema.Table(nameof(GroupPurchaseCommission)).Exists())
        {
            Create.TableFor<GroupPurchaseCommission>();
        }

        if (Schema.Table(nameof(RewardRule)).Exists())
        {
            if (!Schema.Table(nameof(RewardRule)).Column(nameof(RewardRule.MinRewardAmount)).Exists())
            {
                Alter.Table(nameof(RewardRule))
                    .AddColumn(nameof(RewardRule.MinRewardAmount)).AsDecimal(18, 4).Nullable();
            }

            if (!Schema.Table(nameof(RewardRule)).Column(nameof(RewardRule.MaxRewardAmount)).Exists())
            {
                Alter.Table(nameof(RewardRule))
                    .AddColumn(nameof(RewardRule.MaxRewardAmount)).AsDecimal(18, 4).Nullable();
            }
        }
    }
}
