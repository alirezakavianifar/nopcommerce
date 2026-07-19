using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Vendors;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Plugin.Misc.SellerMarketing.Domain;
using Nop.Plugin.Misc.SellerMarketing.Models;
using Nop.Plugin.Misc.SellerMarketing.Services;

namespace Nop.Plugin.Misc.SellerMarketing.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class SellerMarketingAdminController : BasePluginController
{
    #region Fields

    protected readonly ISellerMarketingService _sellerMarketingService;
    protected readonly IRepository<Product> _productRepository;
    protected readonly IRepository<ProductPicture> _productPictureRepository;
    protected readonly IVendorService _vendorService;
    protected readonly IPictureService _pictureService;
    protected readonly INotificationService _notificationService;
    protected readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public SellerMarketingAdminController(
        ISellerMarketingService sellerMarketingService,
        IRepository<Product> productRepository,
        IRepository<ProductPicture> productPictureRepository,
        IVendorService vendorService,
        IPictureService pictureService,
        INotificationService notificationService,
        ILocalizationService localizationService)
    {
        _sellerMarketingService = sellerMarketingService;
        _productRepository = productRepository;
        _productPictureRepository = productPictureRepository;
        _vendorService = vendorService;
        _pictureService = pictureService;
        _notificationService = notificationService;
        _localizationService = localizationService;
    }

    #endregion

    #region Actions

    [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    public async Task<IActionResult> List()
    {
        var model = new AdminSubmissionListModel();
        var submissions = await _sellerMarketingService.GetPendingSubmissionsAsync();

        foreach (var sub in submissions)
        {
            var product = await _productRepository.GetByIdAsync(sub.ProductId);
            if (product == null || product.Deleted)
                continue;

            var vendor = await _vendorService.GetVendorByIdAsync(sub.VendorId);

            // Resolve Image URL
            var pictureMapping = (await _productPictureRepository.GetAllAsync(q => q.Where(pp => pp.ProductId == product.Id))).FirstOrDefault();
            var imageUrl = string.Empty;
            if (pictureMapping != null)
            {
                imageUrl = await _pictureService.GetPictureUrlAsync(pictureMapping.PictureId, 75, true);
            }
            if (string.IsNullOrEmpty(imageUrl))
            {
                imageUrl = await _pictureService.GetDefaultPictureUrlAsync(75);
            }

            model.Submissions.Add(new AdminSubmissionItemModel
            {
                Id = sub.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                VendorId = sub.VendorId,
                VendorName = vendor?.Name ?? "Unknown",
                Sku = product.Sku,
                Price = product.Price,
                ImageUrl = imageUrl,
                Status = sub.Status.ToString(),
                SubmittedOnUtc = sub.SubmittedOnUtc
            });
        }

        return View("~/Plugins/Misc.SellerMarketing/Views/Admin/List.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public async Task<IActionResult> Approve(int id)
    {
        var submission = await _sellerMarketingService.GetSubmissionByIdAsync(id);
        if (submission == null)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Error"));
            return RedirectToAction("List");
        }

        await _sellerMarketingService.ApproveSubmissionAsync(submission.Id);

        _notificationService.SuccessNotification("Seller product catalog has been successfully approved and published.");
        return RedirectToAction("List");
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public async Task<IActionResult> Reject(int id, string comment)
    {
        var submission = await _sellerMarketingService.GetSubmissionByIdAsync(id);
        if (submission == null)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Error"));
            return RedirectToAction("List");
        }

        if (string.IsNullOrEmpty(comment))
        {
            _notificationService.ErrorNotification("Please enter a feedback comment / reason for rejection.");
            return RedirectToAction("List");
        }

        await _sellerMarketingService.RejectSubmissionAsync(submission.Id, comment);

        _notificationService.SuccessNotification("Seller product catalog has been rejected and seller has been notified with feedback.");
        return RedirectToAction("List");
    }

    #endregion
}
