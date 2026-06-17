using System.Linq;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Shipping.ConditionalMethods.Domain;
using Nop.Services.Shipping;

namespace Nop.Plugin.Shipping.ConditionalMethods.Services;

/// <summary>
/// Default implementation of <see cref="IConditionalShippingService"/>
/// </summary>
public class ConditionalShippingService : IConditionalShippingService
{
    private readonly IRepository<ShippingCityMapping> _cityMappingRepository;
    private readonly IRepository<ShippingProductMapping> _productMappingRepository;
    private readonly IRepository<ShippingWarehouseMapping> _warehouseMappingRepository;
    private readonly ConditionalShippingSettings _settings;
    private readonly ICourierApiService _courierApiService;
    private readonly IFreightApiService _freightApiService;

    public ConditionalShippingService(
        IRepository<ShippingCityMapping> cityMappingRepository,
        IRepository<ShippingProductMapping> productMappingRepository,
        IRepository<ShippingWarehouseMapping> warehouseMappingRepository,
        ConditionalShippingSettings settings,
        ICourierApiService courierApiService,
        IFreightApiService freightApiService)
    {
        _cityMappingRepository = cityMappingRepository;
        _productMappingRepository = productMappingRepository;
        _warehouseMappingRepository = warehouseMappingRepository;
        _settings = settings;
        _courierApiService = courierApiService;
        _freightApiService = freightApiService;
    }

    #region City Mappings

    public async Task<IPagedList<ShippingCityMapping>> GetAllCityMappingsAsync(
        ConditionalShippingType? shippingType = null,
        string cityName = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var query = _cityMappingRepository.Table;

        if (shippingType.HasValue)
            query = query.Where(m => m.ShippingTypeId == (int)shippingType.Value);

        if (!string.IsNullOrWhiteSpace(cityName))
            query = query.Where(m => m.CityName.Contains(cityName));

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task<ShippingCityMapping> GetCityMappingByIdAsync(int id)
    {
        return await _cityMappingRepository.GetByIdAsync(id);
    }

    public async Task InsertCityMappingAsync(ShippingCityMapping mapping)
    {
        await _cityMappingRepository.InsertAsync(mapping);
    }

    public async Task UpdateCityMappingAsync(ShippingCityMapping mapping)
    {
        await _cityMappingRepository.UpdateAsync(mapping);
    }

    public async Task DeleteCityMappingAsync(ShippingCityMapping mapping)
    {
        await _cityMappingRepository.DeleteAsync(mapping);
    }

    #endregion

    #region Product Mappings

    public async Task<IPagedList<ShippingProductMapping>> GetAllProductMappingsAsync(
        ConditionalShippingType? shippingType = null,
        int? productId = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var query = _productMappingRepository.Table;

        if (shippingType.HasValue)
            query = query.Where(m => m.ShippingTypeId == (int)shippingType.Value);

        if (productId.HasValue)
            query = query.Where(m => m.ProductId == productId.Value);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task<ShippingProductMapping> GetProductMappingByIdAsync(int id)
    {
        return await _productMappingRepository.GetByIdAsync(id);
    }

    public async Task InsertProductMappingAsync(ShippingProductMapping mapping)
    {
        await _productMappingRepository.InsertAsync(mapping);
    }

    public async Task DeleteProductMappingAsync(ShippingProductMapping mapping)
    {
        await _productMappingRepository.DeleteAsync(mapping);
    }

    #endregion

    #region Warehouse Mappings

    public async Task<IPagedList<ShippingWarehouseMapping>> GetAllWarehouseMappingsAsync(
        ConditionalShippingType? shippingType = null,
        int? warehouseId = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var query = _warehouseMappingRepository.Table;

        if (shippingType.HasValue)
            query = query.Where(m => m.ShippingTypeId == (int)shippingType.Value);

        if (warehouseId.HasValue)
            query = query.Where(m => m.WarehouseId == warehouseId.Value);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task<ShippingWarehouseMapping> GetWarehouseMappingByIdAsync(int id)
    {
        return await _warehouseMappingRepository.GetByIdAsync(id);
    }

    public async Task InsertWarehouseMappingAsync(ShippingWarehouseMapping mapping)
    {
        await _warehouseMappingRepository.InsertAsync(mapping);
    }

    public async Task DeleteWarehouseMappingAsync(ShippingWarehouseMapping mapping)
    {
        await _warehouseMappingRepository.DeleteAsync(mapping);
    }

    #endregion

    #region Eligibility

    public async Task<bool> IsEligibleAsync(ConditionalShippingType shippingType, GetShippingOptionRequest request)
    {
        // Priority 1: City check
        var cityName = request.CityFrom ?? string.Empty;
        var stateProvinceId = request.StateProvinceFrom?.Id ?? 0;

        var cityMappings = await _cityMappingRepository.Table
            .Where(m => m.ShippingTypeId == (int)shippingType
                        && m.IsActive
                        && m.CityName == cityName
                        && (m.StateProvinceId == 0 || m.StateProvinceId == stateProvinceId))
            .ToListAsync();

        if (!cityMappings.Any())
            return false;

        // Priority 2: Product check — ALL products in the request must be mapped
        var productIds = request.Items.Select(i => i.Product.Id).Distinct().ToList();
        var mappedProductIds = await _productMappingRepository.Table
            .Where(m => m.ShippingTypeId == (int)shippingType && productIds.Contains(m.ProductId))
            .Select(m => m.ProductId)
            .ToListAsync();

        if (!productIds.All(id => mappedProductIds.Contains(id)))
            return false;

        // Priority 3: Warehouse check
        // nopCommerce resolves the origin warehouse per request in WarehouseFrom.
        // If no warehouse is assigned to this request, skip the warehouse check.
        var warehouseIds = new List<int>();
        if (request.WarehouseFrom != null && request.WarehouseFrom.Id > 0)
            warehouseIds.Add(request.WarehouseFrom.Id);

        if (warehouseIds.Any())
        {
            var mappedWarehouseIds = await _warehouseMappingRepository.Table
                .Where(m => m.ShippingTypeId == (int)shippingType && warehouseIds.Contains(m.WarehouseId))
                .Select(m => m.WarehouseId)
                .ToListAsync();

            if (!warehouseIds.All(id => mappedWarehouseIds.Contains(id)))
                return false;
        }

        return true;
    }

    #endregion

    #region Rate Calculation

    public async Task<decimal> CalculateExpressRateAsync(GetShippingOptionRequest request)
    {
        // Get courier cost from API (falls back to PostalBaseRate when unavailable)
        var courierCost = await _courierApiService.GetRateAsync(
            request,
            _settings.ExpressCourierApiKey,
            _settings.ExpressCourierApiEndpoint) ?? _settings.ExpressPostalBaseRate;

        var postalBase = _settings.ExpressPostalBaseRate;
        var baseRate = courierCost + postalBase;

        var rawAddition = baseRate * (_settings.ExpressPercentageIncrease / 100m)
                          + _settings.ExpressFixedAddition;

        var addition = Clamp(rawAddition, _settings.ExpressMinAddition, _settings.ExpressMaxAddition);

        return baseRate + addition;
    }

    public async Task<decimal> CalculateTransportationRateAsync(
        GetShippingOptionRequest request, decimal normalRate)
    {
        var baseRate = normalRate > 0 ? normalRate : _settings.TransportationBaseRate;

        var rawDeduction = baseRate * (_settings.TransportationPercentageDecrease / 100m)
                           + _settings.TransportationFixedDeduction;

        var deduction = Clamp(rawDeduction, _settings.TransportationMinDeduction, _settings.TransportationMaxDeduction);

        await Task.CompletedTask;
        return Math.Max(0, baseRate - deduction);
    }

    public async Task<decimal> CalculateFreightRateAsync(GetShippingOptionRequest request)
    {
        var mode = (FreightCostMode)_settings.FreightCostModeId;

        if (mode == FreightCostMode.Api)
        {
            var apiRate = await _freightApiService.GetRateAsync(
                request,
                _settings.FreightApiKey,
                _settings.FreightApiEndpoint);

            return apiRate ?? _settings.FreightFixedRate;
        }

        if (mode == FreightCostMode.Formula)
        {
            var totalWeightKg = request.Items.Sum(i =>
                (decimal)i.Product.Weight * i.GetQuantity());

            // Distance is not available directly in nopCommerce; default to 0 for now.
            // Integrators can extend this to use a geo-distance API.
            const decimal distanceKm = 0m;

            var perKgCost = totalWeightKg * _settings.FreightCostPerKg;
            perKgCost = Clamp(perKgCost, _settings.FreightMinCostPerKg * totalWeightKg,
                _settings.FreightMaxCostPerKg * totalWeightKg);

            var perKmCost = distanceKm * _settings.FreightCostPerKm;
            perKmCost = Clamp(perKmCost, _settings.FreightMinCostPerKm * distanceKm,
                _settings.FreightMaxCostPerKm * distanceKm);

            return perKgCost + perKmCost;
        }

        // Fixed mode
        await Task.CompletedTask;
        return _settings.FreightFixedRate;
    }

    public async Task<decimal?> GetCourierRateAsync(GetShippingOptionRequest request)
    {
        return await _courierApiService.GetRateAsync(
            request,
            _settings.CourierApiKey,
            _settings.CourierApiEndpoint);
    }

    #endregion

    #region Helpers

    private static decimal Clamp(decimal value, decimal min, decimal max)
    {
        if (min > max)
            return value; // misconfigured bounds — do not clamp

        return Math.Max(min, Math.Min(max, value));
    }

    #endregion
}
