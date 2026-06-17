using Nop.Core;

namespace Nop.Plugin.Shipping.ConditionalMethods.Domain;

/// <summary>
/// Represents a mapping between a shipping type and a supporting warehouse
/// </summary>
public class ShippingWarehouseMapping : BaseEntity
{
    /// <summary>
    /// Gets or sets the conditional shipping type
    /// </summary>
    public int ShippingTypeId { get; set; }

    /// <summary>
    /// Gets or sets the warehouse identifier
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// Gets or sets the shipping type
    /// </summary>
    public ConditionalShippingType ShippingType
    {
        get => (ConditionalShippingType)ShippingTypeId;
        set => ShippingTypeId = (int)value;
    }
}
