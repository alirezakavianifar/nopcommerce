using Nop.Core;
using Nop.Plugin.Shipping.ConditionalMethods.Domain;
using Nop.Services.Shipping;

namespace Nop.Plugin.Shipping.ConditionalMethods.Services;

/// <summary>
/// Service for managing conditional shipping mappings and eligibility evaluation
/// </summary>
public interface IConditionalShippingService
{
    #region City Mappings

    Task<IPagedList<ShippingCityMapping>> GetAllCityMappingsAsync(
        ConditionalShippingType? shippingType = null,
        string cityName = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue);

    Task<ShippingCityMapping> GetCityMappingByIdAsync(int id);

    Task InsertCityMappingAsync(ShippingCityMapping mapping);

    Task UpdateCityMappingAsync(ShippingCityMapping mapping);

    Task DeleteCityMappingAsync(ShippingCityMapping mapping);

    #endregion

    #region Product Mappings

    Task<IPagedList<ShippingProductMapping>> GetAllProductMappingsAsync(
        ConditionalShippingType? shippingType = null,
        int? productId = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue);

    Task<ShippingProductMapping> GetProductMappingByIdAsync(int id);

    Task InsertProductMappingAsync(ShippingProductMapping mapping);

    Task DeleteProductMappingAsync(ShippingProductMapping mapping);

    #endregion

    #region Warehouse Mappings

    Task<IPagedList<ShippingWarehouseMapping>> GetAllWarehouseMappingsAsync(
        ConditionalShippingType? shippingType = null,
        int? warehouseId = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue);

    Task<ShippingWarehouseMapping> GetWarehouseMappingByIdAsync(int id);

    Task InsertWarehouseMappingAsync(ShippingWarehouseMapping mapping);

    Task DeleteWarehouseMappingAsync(ShippingWarehouseMapping mapping);

    #endregion

    #region Eligibility

    /// <summary>
    /// Checks all three priority conditions (City → Product → Warehouse) for a shipping type
    /// </summary>
    Task<bool> IsEligibleAsync(ConditionalShippingType shippingType, GetShippingOptionRequest request);

    #endregion

    #region Rate Calculation

    /// <summary>
    /// Calculates the express shipping rate.
    /// base = courierCost + postalBaseRate
    /// addition = Clamp(base * pct/100 + fixed, min, max)
    /// finalRate = base + addition
    /// </summary>
    Task<decimal> CalculateExpressRateAsync(GetShippingOptionRequest request);

    /// <summary>
    /// Calculates the transportation shipping rate.
    /// base = normalRate (from settings baseline or supplied value)
    /// deduction = Clamp(base * pct/100 + fixed, min, max)
    /// finalRate = Max(0, base - deduction)
    /// </summary>
    Task<decimal> CalculateTransportationRateAsync(GetShippingOptionRequest request, decimal normalRate);

    /// <summary>
    /// Calculates the freight/cargo shipping rate using the configured mode.
    /// </summary>
    Task<decimal> CalculateFreightRateAsync(GetShippingOptionRequest request);

    /// <summary>
    /// Gets the courier shipping rate from the courier API.
    /// </summary>
    Task<decimal?> GetCourierRateAsync(GetShippingOptionRequest request);

    #endregion
}
