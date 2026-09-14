using Nop.Core;

namespace Nop.Plugin.Misc.SellerMarketing.Domain;

/// <summary>
/// Represents a seller restore request awaiting admin approval and verification
/// </summary>
public partial class SellerRestoreRequest : BaseEntity
{
    /// <summary>
    /// Gets or sets the vendor identifier
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// Gets or sets the original backup record identifier if restored from an existing server backup
    /// </summary>
    public int? BackupRecordId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the file was uploaded from outside the system
    /// </summary>
    public bool IsExternalUpload { get; set; }

    /// <summary>
    /// Gets or sets the path to the staged/quarantined archive file
    /// </summary>
    public string StagedFilePath { get; set; }

    /// <summary>
    /// Gets or sets the cryptographic SHA-256 hash of the archive
    /// </summary>
    public string FileHashSha256 { get; set; }

    /// <summary>
    /// Gets or sets the status identifier
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the admin review comment or reason for rejection
    /// </summary>
    public string AdminComment { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the request was submitted
    /// </summary>
    public DateTime RequestedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the request was reviewed by admin
    /// </summary>
    public DateTime? ReviewedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the request status
    /// </summary>
    public RestoreRequestStatus Status
    {
        get => (RestoreRequestStatus)StatusId;
        set => StatusId = (int)value;
    }
}
