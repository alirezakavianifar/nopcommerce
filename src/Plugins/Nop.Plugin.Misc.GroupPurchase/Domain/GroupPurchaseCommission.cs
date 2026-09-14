using Nop.Core;

namespace Nop.Plugin.Misc.GroupPurchase.Domain;

/// <summary>
/// Represents a commission configuration for gross profit resolution
/// </summary>
public partial class GroupPurchaseCommission : BaseEntity
{
    /// <summary>
    /// Gets or sets the commission scope identifier (Product, Category, Vendor, Manufacturer)
    /// </summary>
    public int CommissionScopeId { get; set; }

    /// <summary>
    /// Gets or sets the entity identifier associated with the scope
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// Gets or sets the commission percentage (e.g. 15.5 for 15.5%)
    /// </summary>
    public decimal Percentage { get; set; }

    /// <summary>
    /// Gets or sets the entity name or identifier note (for display and logging)
    /// </summary>
    public string EntityName { get; set; }

    /// <summary>
    /// Gets or sets the creation date in UTC
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the last update date in UTC
    /// </summary>
    public DateTime UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the commission scope enum
    /// </summary>
    public CommissionScope Scope
    {
        get => (CommissionScope)CommissionScopeId;
        set => CommissionScopeId = (int)value;
    }
}
