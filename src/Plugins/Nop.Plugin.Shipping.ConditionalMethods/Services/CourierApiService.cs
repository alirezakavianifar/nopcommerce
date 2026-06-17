using Nop.Services.Logging;
using Nop.Services.Shipping;

namespace Nop.Plugin.Shipping.ConditionalMethods.Services;

/// <summary>
/// Stub implementation of the courier API service.
/// Replace the internal logic with actual HTTP calls to the courier provider.
/// </summary>
public class CourierApiService : ICourierApiService
{
    private readonly ILogger _logger;

    public CourierApiService(ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<decimal?> GetRateAsync(GetShippingOptionRequest request, string apiKey, string apiEndpoint)
    {
        // TODO: Replace with actual HTTP call to courier API.
        // Expected request parameters: origin city, destination address, total weight/dimensions.
        // Return null when the API returns an error or no rate is available.
        try
        {
            // Stub: return a fixed test rate
            await Task.CompletedTask;
            return 50_000m; // example: 50,000 currency units
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"[ConditionalMethods] CourierApiService.GetRateAsync failed: {ex.Message}", ex);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsCitySupported(string cityName, int stateProvinceId, string apiKey, string apiEndpoint)
    {
        // TODO: Replace with actual API call to check if the courier serves this city.
        await Task.CompletedTask;
        return true; // stub: always supported
    }
}
