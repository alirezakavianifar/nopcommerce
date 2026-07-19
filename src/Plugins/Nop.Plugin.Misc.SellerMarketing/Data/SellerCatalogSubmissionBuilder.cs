using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.SellerMarketing.Domain;

namespace Nop.Plugin.Misc.SellerMarketing.Data;

public class SellerCatalogSubmissionBuilder : NopEntityBuilder<SellerCatalogSubmission>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(SellerCatalogSubmission.ProductId)).AsInt32().NotNullable()
            .WithColumn(nameof(SellerCatalogSubmission.VendorId)).AsInt32().NotNullable()
            .WithColumn(nameof(SellerCatalogSubmission.StatusId)).AsInt32().NotNullable()
            .WithColumn(nameof(SellerCatalogSubmission.AdminComment)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(SellerCatalogSubmission.SubmittedOnUtc)).AsDateTime().NotNullable()
            .WithColumn(nameof(SellerCatalogSubmission.ReviewedOnUtc)).AsDateTime().Nullable();
    }
}
