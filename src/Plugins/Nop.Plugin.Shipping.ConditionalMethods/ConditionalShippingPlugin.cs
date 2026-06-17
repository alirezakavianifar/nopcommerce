using Nop.Core;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.ConditionalMethods.Components;
using Nop.Plugin.Shipping.ConditionalMethods.Domain;
using Nop.Plugin.Shipping.ConditionalMethods.Services;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Shipping.ConditionalMethods;

/// <summary>
/// Conditional Shipping Methods plugin — implements Courier, Transportation, Freight/Cargo and Express
/// shipping options that are conditionally available based on city, product and warehouse mappings.
/// </summary>
public class ConditionalShippingPlugin : BasePlugin, IShippingRateComputationMethod, IWidgetPlugin, IAdminMenuPlugin
{
    #region Fields

    private readonly IWebHelper _webHelper;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly IConditionalShippingService _conditionalShippingService;
    private readonly ConditionalShippingSettings _settings;

    #endregion

    #region Ctor

    public ConditionalShippingPlugin(
        IWebHelper webHelper,
        ILocalizationService localizationService,
        ISettingService settingService,
        IConditionalShippingService conditionalShippingService,
        ConditionalShippingSettings settings)
    {
        _webHelper = webHelper;
        _localizationService = localizationService;
        _settingService = settingService;
        _conditionalShippingService = conditionalShippingService;
        _settings = settings;
    }

    #endregion

    #region IShippingRateComputationMethod

    /// <summary>
    /// Gets available conditional shipping options for a given shipping request.
    /// Evaluates each enabled type in order (City → Product → Warehouse) and adds
    /// an option only when all three conditions are satisfied.
    /// </summary>
    public async Task<GetShippingOptionResponse> GetShippingOptionsAsync(
        GetShippingOptionRequest getShippingOptionRequest)
    {
        var response = new GetShippingOptionResponse();

        if (getShippingOptionRequest?.Items == null || !getShippingOptionRequest.Items.Any())
        {
            response.AddError("No shipment items provided.");
            return response;
        }

        // ------------------------------------------------------------------
        // Multi-city / multi-warehouse detection
        // Collect the distinct origin cities from all items in this request.
        // nopCommerce builds one GetShippingOptionRequest per warehouse group,
        // so CityFrom will usually be the same for all items here. However we
        // still collect item-level WarehouseIds for the warehouse eligibility
        // check and to provide a useful description in the multi-city scenario.
        // ------------------------------------------------------------------
        var warehouseCities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(getShippingOptionRequest.CityFrom))
            warehouseCities.Add(getShippingOptionRequest.CityFrom.Trim());

        var multiCity = warehouseCities.Count > 1;
        if (multiCity)
        {
            response.ShippingFromMultipleLocations = true;
            // Surface a warning option that the storefront can detect and render specially.
            response.ShippingOptions.Add(new ShippingOption
            {
                ShippingRateComputationMethodSystemName = "Shipping.ConditionalMethods",
                Name = "MultiCityWarning",
                Description =
                    "Your cart contains products from warehouses in different cities. " +
                    "Each city will be shipped separately with its own shipping cost. " +
                    "You can proceed with separate shipments or modify your cart to buy from a single city.",
                Rate = 0m,
                DisplayOrder = 0
            });
        }

        // ------------------------------------------------------------------
        // Express
        // ------------------------------------------------------------------
        if (_settings.ExpressEnabled &&
            await _conditionalShippingService.IsEligibleAsync(ConditionalShippingType.Express, getShippingOptionRequest))
        {
            var rate = await _conditionalShippingService.CalculateExpressRateAsync(getShippingOptionRequest);
            response.ShippingOptions.Add(new ShippingOption
            {
                ShippingRateComputationMethodSystemName = "Shipping.ConditionalMethods",
                Name = await _localizationService.GetResourceAsync("Plugins.Shipping.ConditionalMethods.Express.Name"),
                Description = await _localizationService.GetResourceAsync("Plugins.Shipping.ConditionalMethods.Express.Description"),
                Rate = rate,
                DisplayOrder = 10
            });
        }

        // ------------------------------------------------------------------
        // Transportation
        // ------------------------------------------------------------------
        if (_settings.TransportationEnabled &&
            await _conditionalShippingService.IsEligibleAsync(ConditionalShippingType.Transportation, getShippingOptionRequest))
        {
            // Use the configured baseline rate as the reference normal rate.
            var rate = await _conditionalShippingService.CalculateTransportationRateAsync(
                getShippingOptionRequest,
                _settings.TransportationBaseRate);

            response.ShippingOptions.Add(new ShippingOption
            {
                ShippingRateComputationMethodSystemName = "Shipping.ConditionalMethods",
                Name = await _localizationService.GetResourceAsync("Plugins.Shipping.ConditionalMethods.Transportation.Name"),
                Description = await _localizationService.GetResourceAsync("Plugins.Shipping.ConditionalMethods.Transportation.Description"),
                Rate = rate,
                DisplayOrder = 20
            });
        }

        // ------------------------------------------------------------------
        // Courier
        // ------------------------------------------------------------------
        if (_settings.CourierEnabled &&
            await _conditionalShippingService.IsEligibleAsync(ConditionalShippingType.Courier, getShippingOptionRequest))
        {
            var rate = await _conditionalShippingService.GetCourierRateAsync(getShippingOptionRequest);
            if (rate.HasValue)
            {
                response.ShippingOptions.Add(new ShippingOption
                {
                    ShippingRateComputationMethodSystemName = "Shipping.ConditionalMethods",
                    Name = await _localizationService.GetResourceAsync("Plugins.Shipping.ConditionalMethods.Courier.Name"),
                    Description = await _localizationService.GetResourceAsync("Plugins.Shipping.ConditionalMethods.Courier.Description"),
                    Rate = rate.Value,
                    DisplayOrder = 30
                });
            }
        }

        // ------------------------------------------------------------------
        // Freight / Cargo
        // ------------------------------------------------------------------
        if (_settings.FreightEnabled &&
            await _conditionalShippingService.IsEligibleAsync(ConditionalShippingType.Freight, getShippingOptionRequest))
        {
            var rate = await _conditionalShippingService.CalculateFreightRateAsync(getShippingOptionRequest);
            response.ShippingOptions.Add(new ShippingOption
            {
                ShippingRateComputationMethodSystemName = "Shipping.ConditionalMethods",
                Name = await _localizationService.GetResourceAsync("Plugins.Shipping.ConditionalMethods.Freight.Name"),
                Description = await _localizationService.GetResourceAsync("Plugins.Shipping.ConditionalMethods.Freight.Description"),
                Rate = rate,
                DisplayOrder = 40
            });
        }

        return response;
    }

    /// <summary>
    /// No fixed pre-checkout rate — options are computed dynamically.
    /// </summary>
    public Task<decimal?> GetFixedRateAsync(GetShippingOptionRequest getShippingOptionRequest)
    {
        return Task.FromResult<decimal?>(null);
    }

    /// <summary>
    /// No shipment tracking provided by this plugin.
    /// </summary>
    public Task<IShipmentTracker> GetShipmentTrackerAsync()
    {
        return Task.FromResult<IShipmentTracker>(null);
    }

    #endregion

    #region IWidgetPlugin

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            PublicWidgetZones.CheckoutShippingMethodTop
        });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(MultiCityWarningViewComponent);
    }

    public bool HideInWidgetList => true;

    #endregion

    #region IAdminMenuPlugin

    public async Task ManageSiteMapAsync(AdminMenuItem rootNode)
    {
        var shippingMenu = rootNode.GetItemBySystemName("Shipping");
        if (shippingMenu == null)
        {
            shippingMenu = rootNode.GetItemBySystemName("Configuration");
        }

        if (shippingMenu != null)
        {
            var pluginNode = new AdminMenuItem
            {
                SystemName = "Shipping.ConditionalMethods",
                Title = await _localizationService.GetResourceAsync("Plugins.Shipping.ConditionalMethods.AdminMenu"),
                Url = "/Admin/ConditionalShipping/Configure",
                IconClass = "far fa-dot-circle",
                Visible = true
            };

            var cityNode = new AdminMenuItem
            {
                SystemName = "Shipping.ConditionalMethods.CityMappings",
                Title = await _localizationService.GetResourceAsync("Plugins.Shipping.ConditionalMethods.AdminMenu.CityMappings"),
                Url = "/Admin/ConditionalShipping/CityMappings",
                IconClass = "far fa-circle",
                Visible = true
            };
            pluginNode.ChildNodes.Add(cityNode);

            var productNode = new AdminMenuItem
            {
                SystemName = "Shipping.ConditionalMethods.ProductMappings",
                Title = await _localizationService.GetResourceAsync("Plugins.Shipping.ConditionalMethods.AdminMenu.ProductMappings"),
                Url = "/Admin/ConditionalShipping/ProductMappings",
                IconClass = "far fa-circle",
                Visible = true
            };
            pluginNode.ChildNodes.Add(productNode);

            var warehouseNode = new AdminMenuItem
            {
                SystemName = "Shipping.ConditionalMethods.WarehouseMappings",
                Title = await _localizationService.GetResourceAsync("Plugins.Shipping.ConditionalMethods.AdminMenu.WarehouseMappings"),
                Url = "/Admin/ConditionalShipping/WarehouseMappings",
                IconClass = "far fa-circle",
                Visible = true
            };
            pluginNode.ChildNodes.Add(warehouseNode);

            shippingMenu.ChildNodes.Add(pluginNode);
        }
    }

    #endregion

    #region BasePlugin overrides

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/ConditionalShipping/Configure";
    }

    public override async Task InstallAsync()
    {
        // Save default settings
        await _settingService.SaveSettingAsync(new ConditionalShippingSettings
        {
            ExpressEnabled = false,
            ExpressPercentageIncrease = 25m,
            ExpressFixedAddition = 0m,
            ExpressMinAddition = 0m,
            ExpressMaxAddition = decimal.MaxValue,
            ExpressPostalBaseRate = 0m,

            TransportationEnabled = false,
            TransportationPercentageDecrease = 25m,
            TransportationFixedDeduction = 0m,
            TransportationMinDeduction = 0m,
            TransportationMaxDeduction = decimal.MaxValue,
            TransportationBaseRate = 0m,

            FreightEnabled = false,
            FreightCostModeId = (int)FreightCostMode.Fixed,
            FreightFixedRate = 0m,
            FreightCostPerKg = 0m,
            FreightMinCostPerKg = 0m,
            FreightMaxCostPerKg = decimal.MaxValue,
            FreightCostPerKm = 0m,
            FreightMinCostPerKm = 0m,
            FreightMaxCostPerKm = decimal.MaxValue,

            CourierEnabled = false
        });

        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Shipping.ConditionalMethods.AdminMenu"] = "Conditional Shipping",
            ["Plugins.Shipping.ConditionalMethods.AdminMenu.CityMappings"] = "City Mappings",
            ["Plugins.Shipping.ConditionalMethods.AdminMenu.ProductMappings"] = "Product Mappings",
            ["Plugins.Shipping.ConditionalMethods.AdminMenu.WarehouseMappings"] = "Warehouse Mappings",

            ["Plugins.Shipping.ConditionalMethods.Express.Name"] = "Express Shipping",
            ["Plugins.Shipping.ConditionalMethods.Express.Description"] = "Fast express delivery via courier + postal service",
            ["Plugins.Shipping.ConditionalMethods.Transportation.Name"] = "Transportation Shipping",
            ["Plugins.Shipping.ConditionalMethods.Transportation.Description"] = "Economical transportation shipping",
            ["Plugins.Shipping.ConditionalMethods.Courier.Name"] = "Courier Shipping",
            ["Plugins.Shipping.ConditionalMethods.Courier.Description"] = "Door-to-door courier delivery",
            ["Plugins.Shipping.ConditionalMethods.Freight.Name"] = "Freight / Cargo Shipping",
            ["Plugins.Shipping.ConditionalMethods.Freight.Description"] = "Bulk cargo shipping service",

            ["Plugins.Shipping.ConditionalMethods.Configure.Title"] = "Conditional Shipping Configuration",
            ["Plugins.Shipping.ConditionalMethods.Configure.Express"] = "Express Shipping Settings",
            ["Plugins.Shipping.ConditionalMethods.Configure.Transportation"] = "Transportation Shipping Settings",
            ["Plugins.Shipping.ConditionalMethods.Configure.Courier"] = "Courier Shipping Settings",
            ["Plugins.Shipping.ConditionalMethods.Configure.Freight"] = "Freight / Cargo Shipping Settings",

            ["Plugins.Shipping.ConditionalMethods.Fields.Enabled"] = "Enabled",
            ["Plugins.Shipping.ConditionalMethods.Fields.PercentageIncrease"] = "Percentage Increase (%)",
            ["Plugins.Shipping.ConditionalMethods.Fields.FixedAddition"] = "Fixed Addition",
            ["Plugins.Shipping.ConditionalMethods.Fields.MinAddition"] = "Minimum Addition",
            ["Plugins.Shipping.ConditionalMethods.Fields.MaxAddition"] = "Maximum Addition",
            ["Plugins.Shipping.ConditionalMethods.Fields.PostalBaseRate"] = "Postal / Tipax Base Rate",
            ["Plugins.Shipping.ConditionalMethods.Fields.PercentageDecrease"] = "Percentage Decrease (%)",
            ["Plugins.Shipping.ConditionalMethods.Fields.FixedDeduction"] = "Fixed Deduction",
            ["Plugins.Shipping.ConditionalMethods.Fields.MinDeduction"] = "Minimum Deduction",
            ["Plugins.Shipping.ConditionalMethods.Fields.MaxDeduction"] = "Maximum Deduction",
            ["Plugins.Shipping.ConditionalMethods.Fields.BaseRate"] = "Baseline Normal Shipping Rate",
            ["Plugins.Shipping.ConditionalMethods.Fields.FreightCostMode"] = "Cost Mode",
            ["Plugins.Shipping.ConditionalMethods.Fields.FixedRate"] = "Fixed Rate",
            ["Plugins.Shipping.ConditionalMethods.Fields.CostPerKg"] = "Cost per Kilogram",
            ["Plugins.Shipping.ConditionalMethods.Fields.MinCostPerKg"] = "Min Cost per Kilogram",
            ["Plugins.Shipping.ConditionalMethods.Fields.MaxCostPerKg"] = "Max Cost per Kilogram",
            ["Plugins.Shipping.ConditionalMethods.Fields.CostPerKm"] = "Cost per Kilometer",
            ["Plugins.Shipping.ConditionalMethods.Fields.MinCostPerKm"] = "Min Cost per Kilometer",
            ["Plugins.Shipping.ConditionalMethods.Fields.MaxCostPerKm"] = "Max Cost per Kilometer",
            ["Plugins.Shipping.ConditionalMethods.Fields.ApiKey"] = "API Key",
            ["Plugins.Shipping.ConditionalMethods.Fields.ApiEndpoint"] = "API Endpoint URL",
            ["Plugins.Shipping.ConditionalMethods.Fields.CourierApiKey"] = "Courier API Key",
            ["Plugins.Shipping.ConditionalMethods.Fields.CourierApiEndpoint"] = "Courier API Endpoint",

            ["Plugins.Shipping.ConditionalMethods.CityMappings.Title"] = "City Mappings",
            ["Plugins.Shipping.ConditionalMethods.CityMappings.AddNew"] = "Add New City Mapping",
            ["Plugins.Shipping.ConditionalMethods.Fields.ShippingType"] = "Shipping Type",
            ["Plugins.Shipping.ConditionalMethods.Fields.CityName"] = "City Name",
            ["Plugins.Shipping.ConditionalMethods.Fields.StateProvinceId"] = "State / Province",
            ["Plugins.Shipping.ConditionalMethods.Fields.IsActive"] = "Active",
            ["Plugins.Shipping.ConditionalMethods.Fields.ProductId"] = "Product",
            ["Plugins.Shipping.ConditionalMethods.Fields.WarehouseId"] = "Warehouse",

            ["Plugins.Shipping.ConditionalMethods.ProductMappings.Title"] = "Product Mappings",
            ["Plugins.Shipping.ConditionalMethods.ProductMappings.AddNew"] = "Add New Product Mapping",
            ["Plugins.Shipping.ConditionalMethods.WarehouseMappings.Title"] = "Warehouse Mappings",
            ["Plugins.Shipping.ConditionalMethods.WarehouseMappings.AddNew"] = "Add New Warehouse Mapping"
        });

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await _settingService.DeleteSettingAsync<ConditionalShippingSettings>();
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Shipping.ConditionalMethods");
        await base.UninstallAsync();
    }

    #endregion
}
