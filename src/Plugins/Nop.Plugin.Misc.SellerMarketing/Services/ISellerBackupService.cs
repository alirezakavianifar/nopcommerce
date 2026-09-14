using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Nop.Plugin.Misc.SellerMarketing.Domain;

namespace Nop.Plugin.Misc.SellerMarketing.Services;

/// <summary>
/// Service for generating cryptographically signed seller backups with granular entity permissions and dual storage
/// </summary>
public interface ISellerBackupService
{
    /// <summary>
    /// Creates a signed backup archive for a vendor according to configured entity permissions
    /// </summary>
    Task<SellerBackupRecord> CreateBackupAsync(int vendorId);

    /// <summary>
    /// Gets all backup records for a given vendor
    /// </summary>
    Task<IList<SellerBackupRecord>> GetBackupsByVendorIdAsync(int vendorId);

    /// <summary>
    /// Gets a backup record by its identifier
    /// </summary>
    Task<SellerBackupRecord> GetBackupByIdAsync(int id);

    /// <summary>
    /// Gets file stream for downloading the backup archive
    /// </summary>
    Task<Stream> GetBackupFileStreamAsync(SellerBackupRecord record);

    /// <summary>
    /// Deletes a backup record and its archive file from the server
    /// </summary>
    Task DeleteBackupAsync(SellerBackupRecord record);
}
