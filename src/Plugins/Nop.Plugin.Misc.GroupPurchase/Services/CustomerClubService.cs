using Nop.Plugin.Misc.GroupPurchase.Domain;
using Nop.Services.Configuration;

namespace Nop.Plugin.Misc.GroupPurchase.Services;

/// <summary>
/// Implements dual conversion for Customer Club points:
/// 1. Points to Lottery chances
/// 2. Points to Wallet currency credit
/// </summary>
public class CustomerClubService : ICustomerClubService
{
    private readonly ISettingService _settingService;
    private readonly ILotteryService _lotteryService;
    private readonly IWalletService _walletService;

    public CustomerClubService(
        ISettingService settingService,
        ILotteryService lotteryService,
        IWalletService walletService)
    {
        _settingService = settingService;
        _lotteryService = lotteryService;
        _walletService = walletService;
    }

    public virtual async Task<CustomerClubSettings> GetSettingsAsync()
    {
        return await _settingService.LoadSettingAsync<CustomerClubSettings>();
    }

    public virtual async Task SaveSettingsAsync(CustomerClubSettings settings)
    {
        await _settingService.SaveSettingAsync(settings);
    }

    public virtual async Task<int> ConvertPointsToLotteryChancesAsync(int customerId, int pointsToConvert)
    {
        if (pointsToConvert <= 0)
            return 0;

        var settings = await GetSettingsAsync();
        if (!settings.EnableDualConversion)
            throw new InvalidOperationException("Customer club conversion is currently disabled.");

        var currentPoints = await _lotteryService.GetTotalPointsAsync(customerId);
        if (currentPoints < pointsToConvert)
            throw new InvalidOperationException($"Insufficient points. You have {currentPoints} points.");

        var ratio = settings.PointsToLotteryChanceRatio > 0 ? settings.PointsToLotteryChanceRatio : 10m;
        var chances = (int)Math.Floor(pointsToConvert / ratio);
        if (chances <= 0)
            throw new InvalidOperationException($"Minimum {ratio} points required for 1 lottery chance.");

        var pointsDeducted = (int)(chances * ratio);

        // Deduct points
        await _lotteryService.AddPointsAsync(customerId, -pointsDeducted, LotterySource.ManualAdjustment);

        return chances;
    }

    public virtual async Task<decimal> ConvertPointsToWalletCreditAsync(int customerId, int pointsToConvert)
    {
        if (pointsToConvert <= 0)
            return 0m;

        var settings = await GetSettingsAsync();
        if (!settings.EnableDualConversion)
            throw new InvalidOperationException("Customer club conversion is currently disabled.");

        var currentPoints = await _lotteryService.GetTotalPointsAsync(customerId);
        if (currentPoints < pointsToConvert)
            throw new InvalidOperationException($"Insufficient points. You have {currentPoints} points.");

        var ratio = settings.PointsToCurrencyRatio > 0 ? settings.PointsToCurrencyRatio : 1000m;
        var walletAmount = pointsToConvert * ratio;

        // Deduct points
        await _lotteryService.AddPointsAsync(customerId, -pointsToConvert, LotterySource.ManualAdjustment);

        // Add to customer wallet
        await _walletService.AddTransactionAsync(
            customerId,
            WalletType.GroupReward,
            walletAmount,
            $"Customer club points conversion: {pointsToConvert} points to wallet credit");

        return walletAmount;
    }
}
