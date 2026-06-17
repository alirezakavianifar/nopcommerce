using Nop.Core;

namespace Nop.Plugin.Misc.GroupPurchase.Domain;

/// <summary>
/// Represents a customer wallet
/// </summary>
public partial class CustomerWallet : BaseEntity
{
    /// <summary>
    /// Gets or sets the customer identifier
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the wallet type identifier
    /// </summary>
    public int WalletTypeId { get; set; }

    /// <summary>
    /// Gets or sets the balance
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the last update
    /// </summary>
    public DateTime UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the wallet type
    /// </summary>
    public WalletType WalletType
    {
        get => (WalletType)WalletTypeId;
        set => WalletTypeId = (int)value;
    }
}
