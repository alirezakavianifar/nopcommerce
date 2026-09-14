using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.SellerMarketing.Domain;

namespace Nop.Plugin.Misc.SellerMarketing.Data;

public class SellerRestoreRequestBuilder : NopEntityBuilder<SellerRestoreRequest>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(SellerRestoreRequest.VendorId)).AsInt32().NotNullable()
            .WithColumn(nameof(SellerRestoreRequest.BackupRecordId)).AsInt32().Nullable()
            .WithColumn(nameof(SellerRestoreRequest.IsExternalUpload)).AsBoolean().NotNullable()
            .WithColumn(nameof(SellerRestoreRequest.StagedFilePath)).AsString(1000).NotNullable()
            .WithColumn(nameof(SellerRestoreRequest.FileHashSha256)).AsString(128).NotNullable()
            .WithColumn(nameof(SellerRestoreRequest.StatusId)).AsInt32().NotNullable()
            .WithColumn(nameof(SellerRestoreRequest.AdminComment)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(SellerRestoreRequest.RequestedOnUtc)).AsDateTime().NotNullable()
            .WithColumn(nameof(SellerRestoreRequest.ReviewedOnUtc)).AsDateTime().Nullable();
    }
}
