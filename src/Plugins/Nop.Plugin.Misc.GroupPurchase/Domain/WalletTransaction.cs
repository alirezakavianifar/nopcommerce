using Nop.Core;

namespace Nop.Plugin.Misc.GroupPurchase.Domain;

/// <summary>
/// Represents a wallet transaction
/// </summary>
public partial class WalletTransaction : BaseEntity
{
    /// <summary>
    /// Gets or sets the wallet identifier
    /// </summary>
    public int CustomerWalletId { get; set; }

    /// <summary>
    /// Gets or sets the transaction amount (can be negative for withdrawals)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the transaction message
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the transaction
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }
}
