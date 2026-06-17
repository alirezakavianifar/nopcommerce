using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.GroupPurchase.Models;

public record CustomerWalletSearchModel : BaseSearchModel
{
    public int CustomerId { get; set; }
}

public record CustomerWalletAdminListModel : BasePagedListModel<CustomerWalletAdminModel>
{
}

public record CustomerWalletAdminModel : BaseNopEntityModel
{
    public int CustomerId { get; set; }
    public string CustomerEmail { get; set; }
    public string WalletTypeName { get; set; }
    public decimal Balance { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
}

public record WalletTransactionSearchModel : BaseSearchModel
{
    public int CustomerWalletId { get; set; }
}

public record WalletTransactionModel : BaseNopEntityModel
{
    public int CustomerWalletId { get; set; }
    public decimal Amount { get; set; }
    public string Message { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
