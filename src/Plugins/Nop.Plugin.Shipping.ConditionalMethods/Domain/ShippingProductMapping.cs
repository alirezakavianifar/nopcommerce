using Nop.Core;

namespace Nop.Plugin.Shipping.ConditionalMethods.Domain;

/// <summary>
/// Represents a mapping between a shipping type and an eligible product
/// </summary>
public class ShippingProductMapping : BaseEntity
{
    /// <summary>
    /// Gets or sets the conditional shipping type
    /// </summary>
    public int ShippingTypeId { get; set; }

    /// <summary>
    /// Gets or sets the product identifier
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the shipping type
    /// </summary>
    public ConditionalShippingType ShippingType
    {
        get => (ConditionalShippingType)ShippingTypeId;
        set => ShippingTypeId = (int)value;
    }
}
