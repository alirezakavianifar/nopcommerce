using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Framework.Controllers;
using Nop.Plugin.Misc.SellerMarketing.Domain;
using Nop.Plugin.Misc.SellerMarketing.Services;

namespace Nop.Plugin.Misc.SellerMarketing.Controllers;

[IgnoreAntiforgeryToken]
public class SellerMarketingApiController : BasePluginController
{
    #region Fields

    protected readonly ISellerMarketingService _sellerMarketingService;
    protected readonly IRepository<Product> _productRepository;
    protected readonly IRepository<ProductCategory> _productCategoryRepository;
    protected readonly IRepository<ProductPicture> _productPictureRepository;
    protected readonly ICategoryService _categoryService;
    protected readonly IPictureService _pictureService;
    protected readonly IDownloadService _downloadService;
    protected readonly IUrlRecordService _urlRecordService;
    protected readonly IWorkContext _workContext;
    protected readonly IStoreContext _storeContext;
    protected readonly ILocalizationService _localizationService;
    protected readonly ICustomerService _customerService;
    protected readonly IVendorService _vendorService;
    protected readonly IPriceCalculationService _priceCalculationService;
    protected readonly IPriceFormatter _priceFormatter;
    protected readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public SellerMarketingApiController(
        ISellerMarketingService sellerMarketingService,
        IRepository<Product> productRepository,
        IRepository<ProductCategory> productCategoryRepository,
        IRepository<ProductPicture> productPictureRepository,
        ICategoryService categoryService,
        IPictureService pictureService,
        IDownloadService downloadService,
        IUrlRecordService urlRecordService,
        IWorkContext workContext,
        IStoreContext storeContext,
        ILocalizationService localizationService,
        ICustomerService customerService,
        IVendorService vendorService,
        IPriceCalculationService priceCalculationService,
        IPriceFormatter priceFormatter,
        IWebHelper webHelper)
    {
        _sellerMarketingService = sellerMarketingService;
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
        _productPictureRepository = productPictureRepository;
        _categoryService = categoryService;
        _pictureService = pictureService;
        _downloadService = downloadService;
        _urlRecordService = urlRecordService;
        _workContext = workContext;
        _storeContext = storeContext;
        _localizationService = localizationService;
        _customerService = customerService;
        _vendorService = vendorService;
        _priceCalculationService = priceCalculationService;
        _priceFormatter = priceFormatter;
        _webHelper = webHelper;
    }

    #endregion

    #region Utilities

    protected virtual async Task<(Customer Customer, int VendorId, string ErrorMessage)> ResolveVendorAsync(int? requestedVendorId = null)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (customer != null)
        {
            var isAdmin = await _customerService.IsAdminAsync(customer);
            if (isAdmin)
            {
                var targetVendorId = requestedVendorId.HasValue && requestedVendorId.Value > 0
                    ? requestedVendorId.Value
                    : (customer.VendorId > 0 ? customer.VendorId : 1);
                return (customer, targetVendorId, null);
            }

            if (customer.VendorId > 0)
            {
                if (requestedVendorId.HasValue && requestedVendorId.Value > 0 && requestedVendorId.Value != customer.VendorId)
                {
                    return (customer, 0, "Access denied: cannot submit requests on behalf of other vendors.");
                }
                return (customer, customer.VendorId, null);
            }
        }

        // For external/API client integration with vendorId parameter
        if (requestedVendorId.HasValue && requestedVendorId.Value > 0)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(requestedVendorId.Value);
            if (vendor != null && vendor.Active && !vendor.Deleted)
            {
                return (customer, vendor.Id, null);
            }
        }

        return (customer, 0, "Vendor authentication required. Please sign in as a vendor or provide a valid VendorId.");
    }

    #endregion

    #region Actions

    /// <summary>
    /// Submit a sponsored product marketing or catalog submission request
    /// POST /api/seller-marketing/submit
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SellerMarketingSubmitApiRequest request = null)
    {
        try
        {
            // Bind from form if request was not sent via JSON body
            request ??= new SellerMarketingSubmitApiRequest();
            if (Request.HasFormContentType)
            {
                if (int.TryParse(Request.Form["productId"], out var pid) && pid > 0)
                    request.ProductId = pid;
                if (int.TryParse(Request.Form["vendorId"], out var vid) && vid > 0)
                    request.VendorId = vid;
                if (decimal.TryParse(Request.Form["dailyBudget"], out var budget))
                    request.DailyBudget = budget;
                if (decimal.TryParse(Request.Form["price"], out var price))
                    request.Price = price;
                if (int.TryParse(Request.Form["stockQuantity"], out var stock))
                    request.StockQuantity = stock;
                if (int.TryParse(Request.Form["selectedCategoryId"], out var catId))
                    request.SelectedCategoryId = catId;

                if (!string.IsNullOrEmpty(Request.Form["name"]))
                    request.Name = Request.Form["name"];
                if (!string.IsNullOrEmpty(Request.Form["sku"]))
                    request.Sku = Request.Form["sku"];
                if (!string.IsNullOrEmpty(Request.Form["shortDescription"]))
                    request.ShortDescription = Request.Form["shortDescription"];
                if (!string.IsNullOrEmpty(Request.Form["fullDescription"]))
                    request.FullDescription = Request.Form["fullDescription"];
                if (!string.IsNullOrEmpty(Request.Form["notes"]))
                    request.Notes = Request.Form["notes"];
                if (!string.IsNullOrEmpty(Request.Form["campaignName"]))
                    request.CampaignName = Request.Form["campaignName"];
            }

            var (customer, vendorId, error) = await ResolveVendorAsync(request.VendorId);
            if (!string.IsNullOrEmpty(error) || vendorId <= 0)
            {
                return Json(new { success = false, message = error ?? "Vendor authentication failed." });
            }

            // Case A: Existing product promotion / marketing request
            if (request.ProductId > 0)
            {
                var product = await _productRepository.GetByIdAsync(request.ProductId);
                if (product == null || product.Deleted)
                {
                    return Json(new { success = false, message = $"Product with ID {request.ProductId} was not found." });
                }

                var isAdmin = customer != null && await _customerService.IsAdminAsync(customer);
                if (!isAdmin && product.VendorId != vendorId)
                {
                    return Json(new { success = false, message = "Access denied: product belongs to a different vendor." });
                }

                var existingSubmission = await _sellerMarketingService.GetSubmissionByProductIdAsync(product.Id);
                if (existingSubmission != null)
                {
                    existingSubmission.Status = SubmissionStatus.Pending;
                    existingSubmission.AdminComment = !string.IsNullOrWhiteSpace(request.Notes)
                        ? request.Notes
                        : (request.DailyBudget.HasValue ? $"Daily Budget: {request.DailyBudget:N0}" : existingSubmission.AdminComment);
                    existingSubmission.SubmittedOnUtc = DateTime.UtcNow;
                    await _sellerMarketingService.UpdateSubmissionAsync(existingSubmission);

                    await _sellerMarketingService.SendAdminNotificationAsync(existingSubmission, product);

                    return Json(new
                    {
                        success = true,
                        submissionId = existingSubmission.Id,
                        productId = product.Id,
                        productName = product.Name,
                        status = existingSubmission.Status.ToString(),
                        message = "Marketing request updated and re-submitted for approval."
                    });
                }
                else
                {
                    var newSubmission = new SellerCatalogSubmission
                    {
                        ProductId = product.Id,
                        VendorId = vendorId,
                        Status = SubmissionStatus.Pending,
                        AdminComment = !string.IsNullOrWhiteSpace(request.Notes)
                            ? request.Notes
                            : (request.DailyBudget.HasValue ? $"Daily Budget: {request.DailyBudget:N0}" : string.Empty),
                        SubmittedOnUtc = DateTime.UtcNow
                    };
                    await _sellerMarketingService.InsertSubmissionAsync(newSubmission);
                    await _sellerMarketingService.SendAdminNotificationAsync(newSubmission, product);

                    return Json(new
                    {
                        success = true,
                        submissionId = newSubmission.Id,
                        productId = product.Id,
                        productName = product.Name,
                        status = newSubmission.Status.ToString(),
                        message = "Marketing request submitted successfully."
                    });
                }
            }

            // Case B: Submitting new product catalog item
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Json(new { success = false, message = "Product Name is required when submitting a new catalog item." });
            }

            var newProduct = new Product
            {
                Name = request.Name,
                Sku = request.Sku ?? string.Empty,
                ShortDescription = request.ShortDescription ?? string.Empty,
                FullDescription = request.FullDescription ?? string.Empty,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                VendorId = vendorId,
                Published = false,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                VisibleIndividually = true,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock
            };
            await _productRepository.InsertAsync(newProduct);

            var seName = await _urlRecordService.ValidateSeNameAsync(newProduct, request.Name, request.Name, true);
            await _urlRecordService.SaveSlugAsync(newProduct, seName, 0);

            if (request.SelectedCategoryId > 0)
            {
                await _productCategoryRepository.InsertAsync(new ProductCategory
                {
                    ProductId = newProduct.Id,
                    CategoryId = request.SelectedCategoryId,
                    DisplayOrder = 1
                });
            }

            // Handle file upload if present
            var uploadedFile = Request.HasFormContentType ? Request.Form.Files.FirstOrDefault() : null;
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                var fileBytes = await _downloadService.GetDownloadBitsAsync(uploadedFile);
                var picture = await _pictureService.InsertPictureAsync(fileBytes, uploadedFile.ContentType, null);
                if (picture != null)
                {
                    await _productPictureRepository.InsertAsync(new ProductPicture
                    {
                        ProductId = newProduct.Id,
                        PictureId = picture.Id,
                        DisplayOrder = 1
                    });
                }
            }

            var catalogSubmission = new SellerCatalogSubmission
            {
                ProductId = newProduct.Id,
                VendorId = vendorId,
                Status = SubmissionStatus.Pending,
                AdminComment = request.Notes ?? (request.DailyBudget.HasValue ? $"Daily Budget: {request.DailyBudget:N0}" : string.Empty),
                SubmittedOnUtc = DateTime.UtcNow
            };
            await _sellerMarketingService.InsertSubmissionAsync(catalogSubmission);
            await _sellerMarketingService.SendAdminNotificationAsync(catalogSubmission, newProduct);

            return Json(new
            {
                success = true,
                submissionId = catalogSubmission.Id,
                productId = newProduct.Id,
                productName = newProduct.Name,
                status = catalogSubmission.Status.ToString(),
                message = "Product catalog & marketing submission created successfully and is pending review."
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Fetch active and pending marketing submissions for the authenticated seller
    /// GET /api/seller-marketing/my-requests
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> MyRequests(int? vendorId = null, string status = null, int pageIndex = 0, int pageSize = 50)
    {
        try
        {
            var (customer, targetVendorId, error) = await ResolveVendorAsync(vendorId);
            if (!string.IsNullOrEmpty(error) || targetVendorId <= 0)
            {
                return Json(new { success = false, message = error ?? "Vendor authentication required." });
            }

            var submissions = await _sellerMarketingService.GetSubmissionsByVendorAsync(targetVendorId, pageIndex, pageSize);
            var results = new List<object>();

            var store = await _storeContext.GetCurrentStoreAsync();

            foreach (var sub in submissions)
            {
                if (!string.IsNullOrEmpty(status) && !sub.Status.ToString().Equals(status, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var product = await _productRepository.GetByIdAsync(sub.ProductId);
                if (product == null || product.Deleted)
                    continue;

                // Picture
                var pictureMapping = (await _productPictureRepository.GetAllAsync(q => q.Where(pp => pp.ProductId == product.Id))).FirstOrDefault();
                var imageUrl = string.Empty;
                if (pictureMapping != null)
                {
                    imageUrl = await _pictureService.GetPictureUrlAsync(pictureMapping.PictureId, 120, true);
                }
                if (string.IsNullOrEmpty(imageUrl))
                {
                    imageUrl = await _pictureService.GetDefaultPictureUrlAsync(120);
                }

                // Price formatting
                var (_, finalPrice, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, store);
                var formattedPrice = await _priceFormatter.FormatPriceAsync(finalPrice);

                results.Add(new
                {
                    id = sub.Id,
                    productId = product.Id,
                    productName = product.Name,
                    sku = product.Sku,
                    price = product.Price,
                    finalPrice = finalPrice,
                    formattedPrice = formattedPrice,
                    stockQuantity = product.StockQuantity,
                    published = product.Published,
                    imageUrl = imageUrl,
                    status = sub.Status.ToString(),
                    statusId = sub.StatusId,
                    adminComment = sub.AdminComment,
                    submittedOnUtc = sub.SubmittedOnUtc,
                    reviewedOnUtc = sub.ReviewedOnUtc
                });
            }

            return Json(new
            {
                success = true,
                vendorId = targetVendorId,
                totalCount = results.Count,
                pageIndex = pageIndex,
                pageSize = pageSize,
                requests = results
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    #endregion
}

public class SellerMarketingSubmitApiRequest
{
    public int ProductId { get; set; }
    public decimal? DailyBudget { get; set; }
    public DateTime? StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
    public string CampaignName { get; set; }
    public string Notes { get; set; }

    // Catalog creation fields
    public string Name { get; set; }
    public string Sku { get; set; }
    public string ShortDescription { get; set; }
    public string FullDescription { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int SelectedCategoryId { get; set; }

    // Vendor identifier
    public int? VendorId { get; set; }
}
