using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Services;

/// <summary>
/// Wallet service interface
/// </summary>
public interface IWalletService
{
    /// <summary>
    /// Gets customer wallet
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="walletType">Wallet type</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<CustomerWallet> GetCustomerWalletAsync(int customerId, WalletType walletType);

    /// <summary>
    /// Adds a transaction to the wallet
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="walletType">Wallet type</param>
    /// <param name="amount">Amount (can be negative)</param>
    /// <param name="message">Transaction message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task AddTransactionAsync(int customerId, WalletType walletType, decimal amount, string message);

    /// <summary>
    /// Gets the current wallet balance
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="walletType">Wallet type</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<decimal> GetBalanceAsync(int customerId, WalletType walletType);
}
