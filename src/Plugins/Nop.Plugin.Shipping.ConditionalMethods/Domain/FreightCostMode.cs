namespace Nop.Plugin.Shipping.ConditionalMethods.Domain;

/// <summary>
/// Represents how freight/cargo shipping cost is determined
/// </summary>
public enum FreightCostMode
{
    Fixed = 0,
    Formula = 1,
    Api = 2
}
