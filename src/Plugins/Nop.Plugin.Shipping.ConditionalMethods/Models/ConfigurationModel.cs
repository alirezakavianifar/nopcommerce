using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.ConditionalMethods.Models;

/// <summary>
/// View model for the plugin configuration page
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    #region Express

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.Enabled")]
    public bool ExpressEnabled { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.PercentageIncrease")]
    public decimal ExpressPercentageIncrease { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.FixedAddition")]
    public decimal ExpressFixedAddition { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.MinAddition")]
    public decimal ExpressMinAddition { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.MaxAddition")]
    public decimal ExpressMaxAddition { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.CourierApiKey")]
    public string ExpressCourierApiKey { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.CourierApiEndpoint")]
    public string ExpressCourierApiEndpoint { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.PostalBaseRate")]
    public decimal ExpressPostalBaseRate { get; set; }

    #endregion

    #region Transportation

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.Enabled")]
    public bool TransportationEnabled { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.PercentageDecrease")]
    public decimal TransportationPercentageDecrease { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.FixedDeduction")]
    public decimal TransportationFixedDeduction { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.MinDeduction")]
    public decimal TransportationMinDeduction { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.MaxDeduction")]
    public decimal TransportationMaxDeduction { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.BaseRate")]
    public decimal TransportationBaseRate { get; set; }

    #endregion

    #region Freight

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.Enabled")]
    public bool FreightEnabled { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.FreightCostMode")]
    public int FreightCostModeId { get; set; }

    public IList<SelectListItem> AvailableFreightCostModes { get; set; } = new List<SelectListItem>();

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.FixedRate")]
    public decimal FreightFixedRate { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.CostPerKg")]
    public decimal FreightCostPerKg { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.MinCostPerKg")]
    public decimal FreightMinCostPerKg { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.MaxCostPerKg")]
    public decimal FreightMaxCostPerKg { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.CostPerKm")]
    public decimal FreightCostPerKm { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.MinCostPerKm")]
    public decimal FreightMinCostPerKm { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.MaxCostPerKm")]
    public decimal FreightMaxCostPerKm { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.ApiKey")]
    public string FreightApiKey { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.ApiEndpoint")]
    public string FreightApiEndpoint { get; set; }

    #endregion

    #region Courier

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.Enabled")]
    public bool CourierEnabled { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.ApiKey")]
    public string CourierApiKey { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.ConditionalMethods.Fields.ApiEndpoint")]
    public string CourierApiEndpoint { get; set; }

    #endregion
}
