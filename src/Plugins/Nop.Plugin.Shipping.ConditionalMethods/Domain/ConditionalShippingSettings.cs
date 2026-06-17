using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.ConditionalMethods.Domain;

/// <summary>
/// Represents settings for all conditional shipping method types
/// </summary>
public class ConditionalShippingSettings : ISettings
{
    #region Express

    /// <summary>Whether express shipping is enabled</summary>
    public bool ExpressEnabled { get; set; }

    /// <summary>
    /// Percentage increase applied on top of the base (courier + postal) cost.
    /// E.g. 25 means 25% more than base.
    /// </summary>
    public decimal ExpressPercentageIncrease { get; set; }

    /// <summary>Fixed amount added to the base cost (in addition to percentage)</summary>
    public decimal ExpressFixedAddition { get; set; }

    /// <summary>Minimum addition amount (floor for the calculated addition)</summary>
    public decimal ExpressMinAddition { get; set; }

    /// <summary>Maximum addition amount (ceiling for the calculated addition)</summary>
    public decimal ExpressMaxAddition { get; set; }

    /// <summary>API key for the courier service used to fetch express base rates</summary>
    public string ExpressCourierApiKey { get; set; }

    /// <summary>API endpoint for the courier service</summary>
    public string ExpressCourierApiEndpoint { get; set; }

    /// <summary>Fixed postal/Tipax base rate to use when API is unavailable (fallback)</summary>
    public decimal ExpressPostalBaseRate { get; set; }

    #endregion

    #region Transportation

    /// <summary>Whether transportation shipping is enabled</summary>
    public bool TransportationEnabled { get; set; }

    /// <summary>
    /// Percentage deducted from the normal shipping rate.
    /// E.g. 25 means 25% less than normal shipping.
    /// </summary>
    public decimal TransportationPercentageDecrease { get; set; }

    /// <summary>Fixed amount deducted from the normal shipping rate</summary>
    public decimal TransportationFixedDeduction { get; set; }

    /// <summary>Minimum deduction amount (floor for the calculated deduction)</summary>
    public decimal TransportationMinDeduction { get; set; }

    /// <summary>Maximum deduction amount (ceiling for the calculated deduction)</summary>
    public decimal TransportationMaxDeduction { get; set; }

    /// <summary>
    /// Normal/baseline shipping rate used as reference when no other provider supplies one.
    /// Used only as a fallback.
    /// </summary>
    public decimal TransportationBaseRate { get; set; }

    #endregion

    #region Freight

    /// <summary>Whether freight/cargo shipping is enabled</summary>
    public bool FreightEnabled { get; set; }

    /// <summary>Cost mode: Fixed, Formula, or Api</summary>
    public int FreightCostModeId { get; set; }

    /// <summary>Fixed freight rate (used when FreightCostMode == Fixed)</summary>
    public decimal FreightFixedRate { get; set; }

    /// <summary>Cost per kilogram (used in formula mode)</summary>
    public decimal FreightCostPerKg { get; set; }

    /// <summary>Minimum cost per kilogram bound</summary>
    public decimal FreightMinCostPerKg { get; set; }

    /// <summary>Maximum cost per kilogram bound</summary>
    public decimal FreightMaxCostPerKg { get; set; }

    /// <summary>Cost per kilometer (used in formula mode)</summary>
    public decimal FreightCostPerKm { get; set; }

    /// <summary>Minimum cost per kilometer bound</summary>
    public decimal FreightMinCostPerKm { get; set; }

    /// <summary>Maximum cost per kilometer bound</summary>
    public decimal FreightMaxCostPerKm { get; set; }

    /// <summary>API key for the freight rate service</summary>
    public string FreightApiKey { get; set; }

    /// <summary>API endpoint for the freight rate service</summary>
    public string FreightApiEndpoint { get; set; }

    #endregion

    #region Courier

    /// <summary>Whether courier shipping is enabled</summary>
    public bool CourierEnabled { get; set; }

    /// <summary>API key for the courier rate service</summary>
    public string CourierApiKey { get; set; }

    /// <summary>API endpoint for the courier rate service</summary>
    public string CourierApiEndpoint { get; set; }

    #endregion
}
