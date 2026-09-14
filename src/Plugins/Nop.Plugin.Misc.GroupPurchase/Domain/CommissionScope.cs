namespace Nop.Plugin.Misc.GroupPurchase.Domain;

/// <summary>
/// Represents commission scope priority level
/// </summary>
public enum CommissionScope
{
    /// <summary>
    /// Highest priority: Specific product commission
    /// </summary>
    Product = 10,

    /// <summary>
    /// Second priority: Category commission
    /// </summary>
    Category = 20,

    /// <summary>
    /// Third priority: Vendor commission
    /// </summary>
    Vendor = 30,

    /// <summary>
    /// Fourth priority: Manufacturer / Brand commission
    /// </summary>
    Manufacturer = 40
}
