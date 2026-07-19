using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Messages;
using Nop.Data;
using Nop.Services.Messages;
using Nop.Services.Vendors;
using Nop.Plugin.Misc.SellerMarketing.Domain;

namespace Nop.Plugin.Misc.SellerMarketing.Services;

/// <summary>
/// Seller marketing service implementation
/// </summary>
public class SellerMarketingService : ISellerMarketingService
{
    #region Fields

    protected readonly IRepository<SellerCatalogSubmission> _submissionRepository;
    protected readonly IRepository<Product> _productRepository;
    protected readonly IVendorService _vendorService;
    protected readonly IEmailSender _emailSender;
    protected readonly IEmailAccountService _emailAccountService;
    protected readonly EmailAccountSettings _emailAccountSettings;

    #endregion

    #region Ctor

    public SellerMarketingService(
        IRepository<SellerCatalogSubmission> submissionRepository,
        IRepository<Product> productRepository,
        IVendorService vendorService,
        IEmailSender emailSender,
        IEmailAccountService emailAccountService,
        EmailAccountSettings emailAccountSettings)
    {
        _submissionRepository = submissionRepository;
        _productRepository = productRepository;
        _vendorService = vendorService;
        _emailSender = emailSender;
        _emailAccountService = emailAccountService;
        _emailAccountSettings = emailAccountSettings;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets submissions by vendor identifier
    /// </summary>
    public async Task<IPagedList<SellerCatalogSubmission>> GetSubmissionsByVendorAsync(int vendorId, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _submissionRepository.GetAllPagedAsync(query =>
            query.Where(s => s.VendorId == vendorId).OrderByDescending(s => s.SubmittedOnUtc),
            pageIndex, pageSize);
    }

    /// <summary>
    /// Gets all pending submissions
    /// </summary>
    public async Task<IPagedList<SellerCatalogSubmission>> GetPendingSubmissionsAsync(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _submissionRepository.GetAllPagedAsync(query =>
            query.Where(s => s.StatusId == (int)SubmissionStatus.Pending).OrderByDescending(s => s.SubmittedOnUtc),
            pageIndex, pageSize);
    }

    /// <summary>
    /// Gets submission by product identifier
    /// </summary>
    public async Task<SellerCatalogSubmission> GetSubmissionByProductIdAsync(int productId)
    {
        var query = await _submissionRepository.GetAllAsync(q => q.Where(s => s.ProductId == productId));
        return query.FirstOrDefault();
    }

    /// <summary>
    /// Gets submission by identifier
    /// </summary>
    public async Task<SellerCatalogSubmission> GetSubmissionByIdAsync(int id)
    {
        return await _submissionRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Inserts a submission
    /// </summary>
    public async Task InsertSubmissionAsync(SellerCatalogSubmission submission)
    {
        await _submissionRepository.InsertAsync(submission);
    }

    /// <summary>
    /// Updates a submission
    /// </summary>
    public async Task UpdateSubmissionAsync(SellerCatalogSubmission submission)
    {
        await _submissionRepository.UpdateAsync(submission);
    }

    /// <summary>
    /// Approves a submission (publishes the product)
    /// </summary>
    public async Task ApproveSubmissionAsync(int submissionId)
    {
        var submission = await GetSubmissionByIdAsync(submissionId);
        if (submission == null) return;

        submission.Status = SubmissionStatus.Approved;
        submission.ReviewedOnUtc = DateTime.UtcNow;
        await UpdateSubmissionAsync(submission);

        var product = await _productRepository.GetByIdAsync(submission.ProductId);
        if (product != null)
        {
            product.Published = true;
            await _productRepository.UpdateAsync(product);

            // Send notification to vendor
            await SendVendorNotificationAsync(submission, product, "Approved");
        }
    }

    /// <summary>
    /// Rejects a submission (unpublishes the product and adds comments)
    /// </summary>
    public async Task RejectSubmissionAsync(int submissionId, string comment)
    {
        var submission = await GetSubmissionByIdAsync(submissionId);
        if (submission == null) return;

        submission.Status = SubmissionStatus.Rejected;
        submission.AdminComment = comment;
        submission.ReviewedOnUtc = DateTime.UtcNow;
        await UpdateSubmissionAsync(submission);

        var product = await _productRepository.GetByIdAsync(submission.ProductId);
        if (product != null)
        {
            product.Published = false;
            await _productRepository.UpdateAsync(product);

            // Send notification to vendor
            await SendVendorNotificationAsync(submission, product, "Rejected");
        }
    }

    /// <summary>
    /// Sends notification email to administrator about a new product submission
    /// </summary>
    public async Task SendAdminNotificationAsync(SellerCatalogSubmission submission, Product product)
    {
        try
        {
            var vendor = await _vendorService.GetVendorByIdAsync(submission.VendorId);
            var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)
                               ?? (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
            if (emailAccount == null)
                return;

            var subject = $"New Product Submission Pending Approval - {product.Name}";
            var body = $@"<p>A new product has been submitted for approval by vendor: <strong>{(vendor?.Name ?? "Unknown")}</strong>.</p>
<p><strong>Product:</strong> {product.Name}</p>
<p><strong>SKU:</strong> {product.Sku}</p>
<p><strong>Price:</strong> {product.Price}</p>
<p>Please log in to the administration panel to review and approve/reject this submission.</p>";

            await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, emailAccount.Email, emailAccount.DisplayName);
        }
        catch
        {
            // Fail silently
        }
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Sends notification to vendor about their submission review status
    /// </summary>
    protected virtual async Task SendVendorNotificationAsync(SellerCatalogSubmission submission, Product product, string status)
    {
        try
        {
            var vendor = await _vendorService.GetVendorByIdAsync(submission.VendorId);
            if (vendor == null || string.IsNullOrEmpty(vendor.Email))
                return;

            var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)
                               ?? (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
            if (emailAccount == null)
                return;

            var subject = $"Product Submission {status} - {product.Name}";
            var body = $@"<p>Dear {vendor.Name},</p>
<p>Your product submission for <strong>{product.Name}</strong> (SKU: {product.Sku}) has been <strong>{status.ToLowerInvariant()}</strong> by the store administrator.</p>";

            if (submission.Status == SubmissionStatus.Rejected && !string.IsNullOrEmpty(submission.AdminComment))
            {
                body += $"<p><strong>Reason for rejection/revision request:</strong> {submission.AdminComment}</p>";
            }
            body += "<p>Thank you for selling with us.</p>";

            await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, vendor.Email, vendor.Name);
        }
        catch
        {
            // Fail silently
        }
    }

    #endregion
}
