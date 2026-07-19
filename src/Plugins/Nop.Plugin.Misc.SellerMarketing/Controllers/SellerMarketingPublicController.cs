using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Plugin.Misc.SellerMarketing.Domain;
using Nop.Plugin.Misc.SellerMarketing.Models;
using Nop.Plugin.Misc.SellerMarketing.Services;

namespace Nop.Plugin.Misc.SellerMarketing.Controllers;

[AutoValidateAntiforgeryToken]
public class SellerMarketingPublicController : BasePluginController
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
    protected readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public SellerMarketingPublicController(
        ISellerMarketingService sellerMarketingService,
        IRepository<Product> productRepository,
        IRepository<ProductCategory> productCategoryRepository,
        IRepository<ProductPicture> productPictureRepository,
        ICategoryService categoryService,
        IPictureService pictureService,
        IDownloadService downloadService,
        IUrlRecordService urlRecordService,
        IWorkContext workContext,
        ILocalizationService localizationService)
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
        _localizationService = localizationService;
    }

    #endregion

    #region Utilities

    protected virtual async Task<(Customer Customer, int VendorId, IActionResult RedirectResult)> CheckVendorAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null)
            return (null, 0, Challenge());

        if (customer.VendorId == 0)
            return (customer, 0, RedirectToRoute("Homepage"));

        return (customer, customer.VendorId, null);
    }

    #endregion

    #region Actions

    public async Task<IActionResult> Dashboard()
    {
        var (customer, vendorId, redirectResult) = await CheckVendorAsync();
        if (redirectResult != null)
            return redirectResult;

        var model = new SellerDashboardModel();
        var submissions = await _sellerMarketingService.GetSubmissionsByVendorAsync(vendorId);

        foreach (var sub in submissions)
        {
            var product = await _productRepository.GetByIdAsync(sub.ProductId);
            if (product == null || product.Deleted)
                continue;

            // Resolve Image URL
            var pictureMapping = (await _productPictureRepository.GetAllAsync(q => q.Where(pp => pp.ProductId == product.Id))).FirstOrDefault();
            var imageUrl = string.Empty;
            if (pictureMapping != null)
            {
                imageUrl = await _pictureService.GetPictureUrlAsync(pictureMapping.PictureId, 100, true);
            }
            if (string.IsNullOrEmpty(imageUrl))
            {
                imageUrl = await _pictureService.GetDefaultPictureUrlAsync(100);
            }

            model.Products.Add(new ProductSubmissionItemModel
            {
                Id = sub.Id,
                Name = product.Name,
                Sku = product.Sku,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ImageUrl = imageUrl,
                Status = sub.Status.ToString(),
                AdminComment = sub.AdminComment,
                SubmittedOnUtc = sub.SubmittedOnUtc
            });
        }

        return View("~/Plugins/Misc.SellerMarketing/Views/Public/Dashboard.cshtml", model);
    }

    public async Task<IActionResult> AddProduct()
    {
        var (_, _, redirectResult) = await CheckVendorAsync();
        if (redirectResult != null)
            return redirectResult;

        var model = new ProductSubmissionModel();
        
        // Load categories
        var categories = await _categoryService.GetAllCategoriesAsync();
        model.AvailableCategories = categories.Select(c => new SelectListItem
        {
            Text = c.Name,
            Value = c.Id.ToString()
        }).ToList();

        return View("~/Plugins/Misc.SellerMarketing/Views/Public/AddProduct.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct(ProductSubmissionModel model)
    {
        var (_, vendorId, redirectResult) = await CheckVendorAsync();
        if (redirectResult != null)
            return redirectResult;

        // Image validation if file uploaded
        if (model.ImageFile != null)
        {
            var contentType = model.ImageFile.ContentType.ToLowerInvariant();
            if (!contentType.StartsWith("image/") || contentType.StartsWith("image/svg"))
            {
                ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Plugins.Misc.SellerMarketing.Dashboard.ImageValidation"));
            }
            if (model.ImageFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Plugins.Misc.SellerMarketing.Dashboard.SizeValidation"));
            }
        }

        if (ModelState.IsValid)
        {
            // Create Product
            var product = new Product
            {
                Name = model.Name,
                Sku = model.Sku,
                ShortDescription = model.ShortDescription,
                FullDescription = model.FullDescription,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                VendorId = vendorId,
                Published = false, // Must remain unpublished until review
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                VisibleIndividually = true,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock
            };
            await _productRepository.InsertAsync(product);

            // URL slug record
            var seName = await _urlRecordService.ValidateSeNameAsync(product, model.Name, model.Name, true);
            await _urlRecordService.SaveSlugAsync(product, seName, 0);

            // Category mapping
            if (model.SelectedCategoryId > 0)
            {
                await _productCategoryRepository.InsertAsync(new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = model.SelectedCategoryId,
                    DisplayOrder = 1
                });
            }

            // Image Upload
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var fileBytes = await _downloadService.GetDownloadBitsAsync(model.ImageFile);
                var picture = await _pictureService.InsertPictureAsync(fileBytes, model.ImageFile.ContentType, null);
                if (picture != null)
                {
                    await _productPictureRepository.InsertAsync(new ProductPicture
                    {
                        ProductId = product.Id,
                        PictureId = picture.Id,
                        DisplayOrder = 1
                    });
                }
            }

            // Create Submission
            var submission = new SellerCatalogSubmission
            {
                ProductId = product.Id,
                VendorId = vendorId,
                Status = SubmissionStatus.Pending,
                AdminComment = string.Empty,
                SubmittedOnUtc = DateTime.UtcNow
            };
            await _sellerMarketingService.InsertSubmissionAsync(submission);

            // Notify Admin
            await _sellerMarketingService.SendAdminNotificationAsync(submission, product);

            return RedirectToAction("Dashboard");
        }

        // Re-load categories on error
        var categories = await _categoryService.GetAllCategoriesAsync();
        model.AvailableCategories = categories.Select(c => new SelectListItem
        {
            Text = c.Name,
            Value = c.Id.ToString()
        }).ToList();

        return View("~/Plugins/Misc.SellerMarketing/Views/Public/AddProduct.cshtml", model);
    }

    public async Task<IActionResult> EditProduct(int id)
    {
        var (_, vendorId, redirectResult) = await CheckVendorAsync();
        if (redirectResult != null)
            return redirectResult;

        var submission = await _sellerMarketingService.GetSubmissionByIdAsync(id);
        if (submission == null || submission.VendorId != vendorId)
            return RedirectToAction("Dashboard");

        var product = await _productRepository.GetByIdAsync(submission.ProductId);
        if (product == null || product.Deleted)
            return RedirectToAction("Dashboard");

        var model = new ProductSubmissionModel
        {
            Id = submission.Id,
            Name = product.Name,
            Sku = product.Sku,
            ShortDescription = product.ShortDescription,
            FullDescription = product.FullDescription,
            Price = product.Price,
            StockQuantity = product.StockQuantity
        };

        // Get category mapping
        var categoryMapping = (await _productCategoryRepository.GetAllAsync(q => q.Where(pc => pc.ProductId == product.Id))).FirstOrDefault();
        if (categoryMapping != null)
        {
            model.SelectedCategoryId = categoryMapping.CategoryId;
        }

        // Load categories
        var categories = await _categoryService.GetAllCategoriesAsync();
        model.AvailableCategories = categories.Select(c => new SelectListItem
        {
            Text = c.Name,
            Value = c.Id.ToString(),
            Selected = c.Id == model.SelectedCategoryId
        }).ToList();

        // Get Image URL
        var pictureMapping = (await _productPictureRepository.GetAllAsync(q => q.Where(pp => pp.ProductId == product.Id))).FirstOrDefault();
        if (pictureMapping != null)
        {
            model.ImageUrl = await _pictureService.GetPictureUrlAsync(pictureMapping.PictureId, 150, true);
        }

        return View("~/Plugins/Misc.SellerMarketing/Views/Public/EditProduct.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> EditProduct(ProductSubmissionModel model)
    {
        var (_, vendorId, redirectResult) = await CheckVendorAsync();
        if (redirectResult != null)
            return redirectResult;

        var submission = await _sellerMarketingService.GetSubmissionByIdAsync(model.Id);
        if (submission == null || submission.VendorId != vendorId)
            return RedirectToAction("Dashboard");

        var product = await _productRepository.GetByIdAsync(submission.ProductId);
        if (product == null || product.Deleted)
            return RedirectToAction("Dashboard");

        // Image validation
        if (model.ImageFile != null)
        {
            var contentType = model.ImageFile.ContentType.ToLowerInvariant();
            if (!contentType.StartsWith("image/") || contentType.StartsWith("image/svg"))
            {
                ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Plugins.Misc.SellerMarketing.Dashboard.ImageValidation"));
            }
            if (model.ImageFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Plugins.Misc.SellerMarketing.Dashboard.SizeValidation"));
            }
        }

        if (ModelState.IsValid)
        {
            // Edit Product properties
            product.Name = model.Name;
            product.Sku = model.Sku;
            product.ShortDescription = model.ShortDescription;
            product.FullDescription = model.FullDescription;
            product.Price = model.Price;
            product.StockQuantity = model.StockQuantity;
            product.Published = false; // Reset to unpublished until approved
            product.UpdatedOnUtc = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product);

            // URL slug record update
            var seName = await _urlRecordService.ValidateSeNameAsync(product, model.Name, model.Name, true);
            await _urlRecordService.SaveSlugAsync(product, seName, 0);

            // Category update
            var categoryMapping = (await _productCategoryRepository.GetAllAsync(q => q.Where(pc => pc.ProductId == product.Id))).FirstOrDefault();
            if (categoryMapping != null)
            {
                if (model.SelectedCategoryId > 0)
                {
                    categoryMapping.CategoryId = model.SelectedCategoryId;
                    await _productCategoryRepository.UpdateAsync(categoryMapping);
                }
                else
                {
                    await _productCategoryRepository.DeleteAsync(categoryMapping);
                }
            }
            else if (model.SelectedCategoryId > 0)
            {
                await _productCategoryRepository.InsertAsync(new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = model.SelectedCategoryId,
                    DisplayOrder = 1
                });
            }

            // Image Update
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                // Delete previous images
                var prevMappings = await _productPictureRepository.GetAllAsync(q => q.Where(pp => pp.ProductId == product.Id));
                foreach (var mapping in prevMappings)
                {
                    var prevPicture = await _pictureService.GetPictureByIdAsync(mapping.PictureId);
                    if (prevPicture != null)
                        await _pictureService.DeletePictureAsync(prevPicture);
                    await _productPictureRepository.DeleteAsync(mapping);
                }

                var fileBytes = await _downloadService.GetDownloadBitsAsync(model.ImageFile);
                var picture = await _pictureService.InsertPictureAsync(fileBytes, model.ImageFile.ContentType, null);
                if (picture != null)
                {
                    await _productPictureRepository.InsertAsync(new ProductPicture
                    {
                        ProductId = product.Id,
                        PictureId = picture.Id,
                        DisplayOrder = 1
                    });
                }
            }

            // Reset submission status to Pending for re-review
            submission.Status = SubmissionStatus.Pending;
            submission.AdminComment = string.Empty;
            submission.SubmittedOnUtc = DateTime.UtcNow;
            await _sellerMarketingService.UpdateSubmissionAsync(submission);

            // Notify Admin
            await _sellerMarketingService.SendAdminNotificationAsync(submission, product);

            return RedirectToAction("Dashboard");
        }

        // Re-load categories
        var categories = await _categoryService.GetAllCategoriesAsync();
        model.AvailableCategories = categories.Select(c => new SelectListItem
        {
            Text = c.Name,
            Value = c.Id.ToString(),
            Selected = c.Id == model.SelectedCategoryId
        }).ToList();

        return View("~/Plugins/Misc.SellerMarketing/Views/Public/EditProduct.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var (_, vendorId, redirectResult) = await CheckVendorAsync();
        if (redirectResult != null)
            return redirectResult;

        var submission = await _sellerMarketingService.GetSubmissionByIdAsync(id);
        if (submission == null || submission.VendorId != vendorId)
            return RedirectToAction("Dashboard");

        var product = await _productRepository.GetByIdAsync(submission.ProductId);
        if (product != null)
        {
            product.Deleted = true;
            await _productRepository.UpdateAsync(product);
        }

        // Delete the submission mapping
        await _sellerMarketingService.RejectSubmissionAsync(submission.Id, "Product Deleted by Vendor");
        
        return RedirectToAction("Dashboard");
    }

    #endregion
}
