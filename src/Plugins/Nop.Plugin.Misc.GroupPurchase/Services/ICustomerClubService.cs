using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Services;

/// <summary>
/// Service for Customer Club point conversions (Lottery chances & Wallet credit)
/// </summary>
public interface ICustomerClubService
{
    /// <summary>
    /// Converts points to lottery chances based on configured ratio
    /// </summary>
    Task<int> ConvertPointsToLotteryChancesAsync(int customerId, int pointsToConvert);

    /// <summary>
    /// Converts points to cash/wallet credit based on configured ratio
    /// </summary>
    Task<decimal> ConvertPointsToWalletCreditAsync(int customerId, int pointsToConvert);

    /// <summary>
    /// Gets customer club conversion settings
    /// </summary>
    Task<CustomerClubSettings> GetSettingsAsync();

    /// <summary>
    /// Saves customer club conversion settings
    /// </summary>
    Task SaveSettingsAsync(CustomerClubSettings settings);
}
