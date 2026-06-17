using Nop.Services.Shipping;

namespace Nop.Plugin.Shipping.ConditionalMethods.Services;

/// <summary>
/// Interface for interacting with an external courier rate/availability API
/// </summary>
public interface ICourierApiService
{
    /// <summary>
    /// Gets the shipping rate from the courier service for the given shipping request
    /// </summary>
    /// <param name="request">The shipping option request</param>
    /// <param name="apiKey">The API key to use</param>
    /// <param name="apiEndpoint">The endpoint URL to call</param>
    /// <returns>The rate returned by the courier API, or null if unavailable</returns>
    Task<decimal?> GetRateAsync(GetShippingOptionRequest request, string apiKey, string apiEndpoint);

    /// <summary>
    /// Checks whether the courier service is available for a given origin city and state/province
    /// </summary>
    /// <param name="cityName">Origin city name</param>
    /// <param name="stateProvinceId">Origin state/province identifier</param>
    /// <param name="apiKey">The API key to use</param>
    /// <param name="apiEndpoint">The endpoint URL to call</param>
    /// <returns>True if the city has courier service</returns>
    Task<bool> IsCitySupported(string cityName, int stateProvinceId, string apiKey, string apiEndpoint);
}
