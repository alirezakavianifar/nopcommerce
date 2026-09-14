using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.SellerMarketing.Services;

/// <summary>
/// Antivirus and archive integrity scanner
/// </summary>
public class AntivirusScanner : IAntivirusScanner
{
    private readonly ILogger _logger;

    private static readonly HashSet<string> DangerousExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".bat", ".cmd", ".ps1", ".vbs", ".js", ".sh", ".php",
        ".asp", ".aspx", ".py", ".jar", ".com", ".scr", ".msi", ".reg", ".pif"
    };

    public AntivirusScanner(ILogger logger = null)
    {
        _logger = logger;
    }

    public virtual async Task<(bool isClean, string errorReason)> ScanArchiveAsync(string archiveFilePath)
    {
        if (string.IsNullOrEmpty(archiveFilePath) || !File.Exists(archiveFilePath))
            return (false, "File not found.");

        try
        {
            // 1. File size check (Max 100MB)
            var fileInfo = new FileInfo(archiveFilePath);
            if (fileInfo.Length > 100 * 1024 * 1024)
                return (false, "File exceeds maximum permitted size of 100MB.");

            // 2. Open ZIP and inspect every entry
            long totalUncompressedBytes = 0;
            using (var archive = ZipFile.OpenRead(archiveFilePath))
            {
                foreach (var entry in archive.Entries)
                {
                    // Check for Zip Slip (path traversal)
                    if (entry.FullName.Contains("..") || entry.FullName.StartsWith("/") || entry.FullName.StartsWith("\\"))
                    {
                        if (_logger != null) await _logger.WarningAsync($"Malicious zip entry detected (path traversal): {entry.FullName}");
                        return (false, $"Archive contains invalid or malicious path: {entry.FullName}");
                    }

                    // Check for dangerous extensions
                    var ext = Path.GetExtension(entry.Name);
                    if (!string.IsNullOrEmpty(ext) && DangerousExtensions.Contains(ext))
                    {
                        if (_logger != null) await _logger.WarningAsync($"Malicious file extension detected in archive: {entry.FullName}");
                        return (false, $"Archive contains forbidden file type: {ext}");
                    }

                    totalUncompressedBytes += entry.Length;
                    // Zip bomb check: max 500MB uncompressed
                    if (totalUncompressedBytes > 500 * 1024 * 1024)
                    {
                        return (false, "Archive expanded size exceeds safe decompression limits (potential zip bomb).");
                    }
                }
            }

            // 3. Optional host-level Windows Defender check if available
            var defenderPath = @"C:\Program Files\Windows Defender\MpCmdRun.exe";
            if (File.Exists(defenderPath))
            {
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = defenderPath,
                        Arguments = $"-Scan -ScanType 3 -File \"{archiveFilePath}\" -DisableRemediation",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    };

                    using var process = Process.Start(startInfo);
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        // Exit code 0 means clean, 2 means threat found
                        if (process.ExitCode == 2)
                        {
                            if (_logger != null) await _logger.ErrorAsync($"Windows Defender flagged malware in file: {archiveFilePath}");
                            return (false, "Security threat identified by antivirus scanner.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Non-fatal if defender command execution lacks permission in some hosted environments
                    if (_logger != null) await _logger.InformationAsync($"Antivirus CLI scanner skipped: {ex.Message}");
                }
            }

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            if (_logger != null) await _logger.ErrorAsync($"Error scanning archive: {ex.Message}", ex);
            return (false, $"Archive integrity verification failed: {ex.Message}");
        }
    }
}
