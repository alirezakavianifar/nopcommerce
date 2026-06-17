using Nop.Services.Shipping;

namespace Nop.Plugin.Shipping.ConditionalMethods.Services;

/// <summary>
/// Interface for interacting with an external freight/cargo rate API
/// </summary>
public interface IFreightApiService
{
    /// <summary>
    /// Gets the freight rate from the external freight API
    /// </summary>
    /// <param name="request">The shipping option request</param>
    /// <param name="apiKey">The API key to use</param>
    /// <param name="apiEndpoint">The endpoint URL to call</param>
    /// <returns>The freight rate, or null if unavailable</returns>
    Task<decimal?> GetRateAsync(GetShippingOptionRequest request, string apiKey, string apiEndpoint);
}
