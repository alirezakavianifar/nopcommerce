namespace Nop.Plugin.Misc.GroupPurchase.Domain;

/// <summary>
/// Represents visibility type for group purchase members
/// </summary>
public enum VisibilityType
{
    /// <summary>
    /// Full visibility
    /// </summary>
    Full = 10,

    /// <summary>
    /// Limited visibility (e.g., first 5 items)
    /// </summary>
    Limited = 20,

    /// <summary>
    /// No visibility
    /// </summary>
    None = 30
}
