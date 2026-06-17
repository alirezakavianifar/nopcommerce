using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Data;

public class GroupPurchaseMemberBuilder : NopEntityBuilder<GroupPurchaseMember>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(GroupPurchaseMember.GroupPurchaseId)).AsInt32().NotNullable()
            .WithColumn(nameof(GroupPurchaseMember.CustomerId)).AsInt32().NotNullable()
            .WithColumn(nameof(GroupPurchaseMember.IsLeader)).AsBoolean().NotNullable()
            .WithColumn(nameof(GroupPurchaseMember.AcceptedTerms)).AsBoolean().NotNullable()
            .WithColumn(nameof(GroupPurchaseMember.AcceptedOnUtc)).AsDateTime().Nullable()
            .WithColumn(nameof(GroupPurchaseMember.VisibilityTypeId)).AsInt32().NotNullable();
    }
}
