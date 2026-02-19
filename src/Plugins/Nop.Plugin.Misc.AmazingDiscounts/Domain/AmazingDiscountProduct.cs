using Nop.Core;

namespace Nop.Plugin.Misc.AmazingDiscounts.Domain;

/// <summary>
/// Represents an amazing discount product
/// </summary>
public partial class AmazingDiscountProduct : BaseEntity
{
    /// <summary>
    /// Gets or sets the product identifier
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the display order
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets the custom label
    /// </summary>
    public string CustomLabel { get; set; }

    /// <summary>
    /// Gets or sets the start date and time in UTC
    /// </summary>
    public DateTime? StartDateUtc { get; set; }

    /// <summary>
    /// Gets or sets the end date and time in UTC
    /// </summary>
    public DateTime? EndDateUtc { get; set; }
}
