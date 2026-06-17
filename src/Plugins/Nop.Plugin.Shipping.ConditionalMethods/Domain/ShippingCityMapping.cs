using Nop.Core;

namespace Nop.Plugin.Shipping.ConditionalMethods.Domain;

/// <summary>
/// Represents a mapping between a shipping type and a supported city
/// </summary>
public class ShippingCityMapping : BaseEntity
{
    /// <summary>
    /// Gets or sets the conditional shipping type
    /// </summary>
    public int ShippingTypeId { get; set; }

    /// <summary>
    /// Gets or sets the city name (string, matched against Address.City)
    /// </summary>
    public string CityName { get; set; }

    /// <summary>
    /// Gets or sets the state/province identifier (optional, for disambiguation)
    /// </summary>
    public int StateProvinceId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this mapping is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the shipping type
    /// </summary>
    public ConditionalShippingType ShippingType
    {
        get => (ConditionalShippingType)ShippingTypeId;
        set => ShippingTypeId = (int)value;
    }
}
