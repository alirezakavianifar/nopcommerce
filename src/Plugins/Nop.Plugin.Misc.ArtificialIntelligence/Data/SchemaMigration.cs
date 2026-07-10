using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.ArtificialIntelligence.Domain;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Data;

[NopMigration("2026/07/10 12:00:00:0000000", "ArtificialIntelligence base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<AiDuplicateProductQueue>();
        Create.TableFor<ProductEmbeddingCache>();
    }
}
