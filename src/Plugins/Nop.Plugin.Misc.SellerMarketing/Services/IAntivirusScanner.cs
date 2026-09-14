using System.IO;

namespace Nop.Plugin.Misc.SellerMarketing.Services;

/// <summary>
/// Security scanner for backup zip archives to protect against Zip Slip, malware, and malicious file payloads
/// </summary>
public interface IAntivirusScanner
{
    /// <summary>
    /// Scans an uploaded backup archive for security vulnerabilities, forbidden executable extensions, and zip slip paths
    /// </summary>
    /// <param name="archiveFilePath">Path to the archive file</param>
    /// <returns>True if clean and safe, false otherwise</returns>
    Task<(bool isClean, string errorReason)> ScanArchiveAsync(string archiveFilePath);
}
