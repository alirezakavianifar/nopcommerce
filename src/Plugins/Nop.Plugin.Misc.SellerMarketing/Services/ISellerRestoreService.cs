using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Plugin.Misc.SellerMarketing.Domain;

namespace Nop.Plugin.Misc.SellerMarketing.Services;

/// <summary>
/// Service for validating and executing vendor restore requests with cryptographic anti-tamper checking and admin approval gates
/// </summary>
public interface ISellerRestoreService
{
    /// <summary>
    /// Creates a restore request for an existing server backup
    /// </summary>
    Task<SellerRestoreRequest> CreateRestoreRequestFromExistingBackupAsync(int vendorId, int backupRecordId);

    /// <summary>
    /// Creates a restore request from an uploaded external backup archive (if permitted by policy)
    /// </summary>
    Task<SellerRestoreRequest> CreateRestoreRequestFromUploadedFileAsync(int vendorId, IFormFile file);

    /// <summary>
    /// Approves and executes a restore request
    /// </summary>
    Task<bool> ApproveAndExecuteRestoreAsync(int requestId, string adminComment);

    /// <summary>
    /// Rejects a restore request
    /// </summary>
    Task<bool> RejectRestoreAsync(int requestId, string adminComment);

    /// <summary>
    /// Gets paged list of restore requests
    /// </summary>
    Task<IPagedList<SellerRestoreRequest>> GetAllRestoreRequestsAsync(int? vendorId = null, RestoreRequestStatus? status = null, int pageIndex = 0, int pageSize = int.MaxValue);

    /// <summary>
    /// Gets a restore request by identifier
    /// </summary>
    Task<SellerRestoreRequest> GetRestoreRequestByIdAsync(int id);
}
