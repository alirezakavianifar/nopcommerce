using Nop.Core;

namespace Nop.Plugin.Misc.SellerMarketing.Domain;

/// <summary>
/// Represents a seller catalog submission entry
/// </summary>
public partial class SellerCatalogSubmission : BaseEntity
{
    /// <summary>
    /// Gets or sets the product identifier
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the vendor identifier
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// Gets or sets the status identifier
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the administrator review comments/reason for rejection
    /// </summary>
    public string AdminComment { get; set; }

    /// <summary>
    /// Gets or sets the submission date
    /// </summary>
    public DateTime SubmittedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the date when the administrator reviewed the product
    /// </summary>
    public DateTime? ReviewedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the submission status
    /// </summary>
    public SubmissionStatus Status
    {
        get => (SubmissionStatus)StatusId;
        set => StatusId = (int)value;
    }
}
