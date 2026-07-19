using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.MultiFactorAuth.SMS.Domains;

namespace Nop.Plugin.MultiFactorAuth.SMS.Migrations;

[NopMigration("2026/07/19 12:00:00", "Nop.Plugin.MultiFactorAuth.SMS schema", MigrationProcessType.Installation)]
public class SMSMigration : AutoReversingMigration
{
    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        Create.TableFor<SMSVerificationRecord>();
        Create.TableFor<CustomerSecurityRestriction>();
    }
}
