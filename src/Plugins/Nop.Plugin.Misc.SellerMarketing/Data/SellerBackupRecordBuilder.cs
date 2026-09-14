using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.SellerMarketing.Domain;

namespace Nop.Plugin.Misc.SellerMarketing.Data;

public class SellerBackupRecordBuilder : NopEntityBuilder<SellerBackupRecord>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(SellerBackupRecord.VendorId)).AsInt32().NotNullable()
            .WithColumn(nameof(SellerBackupRecord.FileName)).AsString(400).NotNullable()
            .WithColumn(nameof(SellerBackupRecord.FilePath)).AsString(1000).NotNullable()
            .WithColumn(nameof(SellerBackupRecord.FileHashSha256)).AsString(128).NotNullable()
            .WithColumn(nameof(SellerBackupRecord.HmacSignature)).AsString(256).NotNullable()
            .WithColumn(nameof(SellerBackupRecord.IncludedEntities)).AsString(500).NotNullable()
            .WithColumn(nameof(SellerBackupRecord.FileSizeBytes)).AsInt64().NotNullable()
            .WithColumn(nameof(SellerBackupRecord.CreatedOnUtc)).AsDateTime().NotNullable();
    }
}
