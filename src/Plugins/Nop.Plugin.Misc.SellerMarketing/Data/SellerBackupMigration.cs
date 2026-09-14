using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.SellerMarketing.Domain;

namespace Nop.Plugin.Misc.SellerMarketing.Data;

[NopMigration("2026/09/14 12:00:00:0000000", "SellerMarketing backup and restore schema", MigrationProcessType.Update)]
public class SellerBackupMigration : AutoReversingMigration
{
    public override void Up()
    {
        if (!Schema.Table(nameof(SellerBackupRecord)).Exists())
        {
            Create.TableFor<SellerBackupRecord>();
        }

        if (!Schema.Table(nameof(SellerRestoreRequest)).Exists())
        {
            Create.TableFor<SellerRestoreRequest>();
        }
    }
}
