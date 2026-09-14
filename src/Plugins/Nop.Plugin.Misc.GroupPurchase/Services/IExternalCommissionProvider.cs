namespace Nop.Plugin.Misc.GroupPurchase.Services;

/// <summary>
/// Interface for external commission and sync providers (e.g., Foroush Gostar integration)
/// </summary>
public interface IExternalCommissionProvider
{
    /// <summary>
    /// Try to resolve commission percentage from external system
    /// </summary>
    Task<decimal?> GetExternalCommissionPercentageAsync(int productId, int vendorId);

    /// <summary>
    /// Synchronize commission tables or settings from external ERP/platform
    /// </summary>
    Task<bool> SyncFromExternalSystemAsync();
}
