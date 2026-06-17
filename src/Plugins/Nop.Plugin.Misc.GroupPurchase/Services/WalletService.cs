using Nop.Data;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Services;

/// <summary>
/// Wallet service
/// </summary>
public class WalletService : IWalletService
{
    #region Fields

    private readonly IRepository<CustomerWallet> _customerWalletRepository;
    private readonly IRepository<WalletTransaction> _walletTransactionRepository;

    #endregion

    #region Ctor

    public WalletService(
        IRepository<CustomerWallet> customerWalletRepository,
        IRepository<WalletTransaction> walletTransactionRepository)
    {
        _customerWalletRepository = customerWalletRepository;
        _walletTransactionRepository = walletTransactionRepository;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets customer wallet
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="walletType">Wallet type</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<CustomerWallet> GetCustomerWalletAsync(int customerId, WalletType walletType)
    {
        if (customerId == 0)
            return null;

        var walletTypeId = (int)walletType;
        var wallet = (await _customerWalletRepository.GetAllAsync(query =>
            query.Where(cw => cw.CustomerId == customerId && cw.WalletTypeId == walletTypeId)
        )).FirstOrDefault();

        if (wallet == null)
        {
            wallet = new CustomerWallet
            {
                CustomerId = customerId,
                WalletTypeId = walletTypeId,
                Balance = 0,
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _customerWalletRepository.InsertAsync(wallet);
        }

        return wallet;
    }

    /// <summary>
    /// Adds a transaction to the wallet
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="walletType">Wallet type</param>
    /// <param name="amount">Amount (can be negative)</param>
    /// <param name="message">Transaction message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task AddTransactionAsync(int customerId, WalletType walletType, decimal amount, string message)
    {
        var wallet = await GetCustomerWalletAsync(customerId, walletType);

        var transaction = new WalletTransaction
        {
            CustomerWalletId = wallet.Id,
            Amount = amount,
            Message = message,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _walletTransactionRepository.InsertAsync(transaction);

        wallet.Balance += amount;
        wallet.UpdatedOnUtc = DateTime.UtcNow;
        await _customerWalletRepository.UpdateAsync(wallet);
    }

    /// <summary>
    /// Gets the current wallet balance
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="walletType">Wallet type</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<decimal> GetBalanceAsync(int customerId, WalletType walletType)
    {
        var wallet = await GetCustomerWalletAsync(customerId, walletType);
        return wallet.Balance;
    }

    #endregion
}
