using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.SellerMarketing.Domain;

/// <summary>
/// Settings for Seller Backup & Restore subsystem
/// </summary>
public class SellerBackupSettings : ISettings
{
    /// <summary>
    /// Whether vendors can backup their products
    /// </summary>
    public bool AllowBackupProducts { get; set; } = true;

    /// <summary>
    /// Whether vendors can backup product attributes and combinations
    /// </summary>
    public bool AllowBackupProductAttributes { get; set; } = true;

    /// <summary>
    /// Whether vendors can backup product pictures
    /// </summary>
    public bool AllowBackupPictures { get; set; } = true;

    /// <summary>
    /// Whether vendors can backup orders (strictly restricted by default per client requirement)
    /// </summary>
    public bool AllowBackupOrders { get; set; } = false;

    /// <summary>
    /// Whether vendors can backup financial/settlement records (strictly restricted by default per client requirement)
    /// </summary>
    public bool AllowBackupFinancialData { get; set; } = false;

    /// <summary>
    /// Admin setting: whether restore can only be done from internal server backups or external file uploads are allowed
    /// </summary>
    public bool AllowExternalFileRestore { get; set; } = false;

    /// <summary>
    /// Whether every restore request strictly requires admin approval
    /// </summary>
    public bool RequireAdminApprovalForRestore { get; set; } = true;

    /// <summary>
    /// Secret key used for cryptographic HMAC-SHA256 signature verification of backup files
    /// </summary>
    public string HmacSignatureSecretKey { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Relative server directory where backups are stored
    /// </summary>
    public string BackupDirectoryPath { get; set; } = "SellerBackups";
}
