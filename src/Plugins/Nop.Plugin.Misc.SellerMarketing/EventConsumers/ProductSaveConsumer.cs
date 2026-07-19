using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Plugin.Misc.SellerMarketing.Domain;
using Nop.Plugin.Misc.SellerMarketing.Services;

namespace Nop.Plugin.Misc.SellerMarketing.EventConsumers;

public class ProductSaveConsumer : IConsumer<EntityInsertedEvent<Product>>, IConsumer<EntityUpdatedEvent<Product>>
{
    #region Fields

    protected readonly ISellerMarketingService _sellerMarketingService;
    protected readonly IRepository<Product> _productRepository;
    protected readonly IWorkContext _workContext;
    protected readonly ICustomerService _customerService;

    #endregion

    #region Ctor

    public ProductSaveConsumer(
        ISellerMarketingService sellerMarketingService,
        IRepository<Product> productRepository,
        IWorkContext workContext,
        ICustomerService customerService)
    {
        _sellerMarketingService = sellerMarketingService;
        _productRepository = productRepository;
        _workContext = workContext;
        _customerService = customerService;
    }

    #endregion

    #region Methods

    public async Task HandleEventAsync(EntityInsertedEvent<Product> eventMessage)
    {
        var product = eventMessage.Entity;
        if (product == null || product.VendorId == 0)
            return;

        await ProcessProductApprovalAsync(product);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
    {
        var product = eventMessage.Entity;
        if (product == null || product.VendorId == 0)
            return;

        await ProcessProductApprovalAsync(product);
    }

    #endregion

    #region Utilities

    protected virtual async Task ProcessProductApprovalAsync(Product product)
    {
        // 1. If deleted, ignore
        if (product.Deleted)
            return;

        // 2. Check if the save action is by an Admin
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        if (currentCustomer == null)
            return;

        var isAdmin = await _customerService.IsAdminAsync(currentCustomer);
        if (isAdmin)
        {
            // Admins can publish products, so we do not override their changes
            return;
        }

        // 3. This is a vendor action. Enforce unpublishing and queue submission.
        var submission = await _sellerMarketingService.GetSubmissionByProductIdAsync(product.Id);

        if (submission == null)
        {
            // Create a new submission
            submission = new SellerCatalogSubmission
            {
                ProductId = product.Id,
                VendorId = product.VendorId,
                Status = SubmissionStatus.Pending,
                AdminComment = string.Empty,
                SubmittedOnUtc = DateTime.UtcNow
            };
            await _sellerMarketingService.InsertSubmissionAsync(submission);

            // Notify admin
            await _sellerMarketingService.SendAdminNotificationAsync(submission, product);
        }
        else
        {
            // If already exists, and status is not Pending, reset it to Pending (vendor edited it)
            if (submission.Status != SubmissionStatus.Pending)
            {
                submission.Status = SubmissionStatus.Pending;
                submission.AdminComment = string.Empty;
                submission.SubmittedOnUtc = DateTime.UtcNow;
                await _sellerMarketingService.UpdateSubmissionAsync(submission);

                // Notify admin
                await _sellerMarketingService.SendAdminNotificationAsync(submission, product);
            }
        }

        // Force product to be unpublished
        if (product.Published)
        {
            product.Published = false;
            await _productRepository.UpdateAsync(product);
        }
    }

    #endregion
}
