using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.SellerMarketing.Domain;

namespace Nop.Plugin.Misc.SellerMarketing.Services;

/// <summary>
/// Seller marketing service interface
/// </summary>
public interface ISellerMarketingService
{
    /// <summary>
    /// Gets submissions by vendor identifier
    /// </summary>
    Task<IPagedList<SellerCatalogSubmission>> GetSubmissionsByVendorAsync(int vendorId, int pageIndex = 0, int pageSize = int.MaxValue);

    /// <summary>
    /// Gets all pending submissions
    /// </summary>
    Task<IPagedList<SellerCatalogSubmission>> GetPendingSubmissionsAsync(int pageIndex = 0, int pageSize = int.MaxValue);

    /// <summary>
    /// Gets submission by product identifier
    /// </summary>
    Task<SellerCatalogSubmission> GetSubmissionByProductIdAsync(int productId);

    /// <summary>
    /// Gets submission by identifier
    /// </summary>
    Task<SellerCatalogSubmission> GetSubmissionByIdAsync(int id);

    /// <summary>
    /// Inserts a submission
    /// </summary>
    Task InsertSubmissionAsync(SellerCatalogSubmission submission);

    /// <summary>
    /// Updates a submission
    /// </summary>
    Task UpdateSubmissionAsync(SellerCatalogSubmission submission);

    /// <summary>
    /// Approves a submission (publishes the product)
    /// </summary>
    Task ApproveSubmissionAsync(int submissionId);

    /// <summary>
    /// Rejects a submission (unpublishes the product and adds comments)
    /// </summary>
    Task RejectSubmissionAsync(int submissionId, string comment);

    /// <summary>
    /// Sends notification email to administrator about a new product submission
    /// </summary>
    Task SendAdminNotificationAsync(SellerCatalogSubmission submission, Product product);
}
