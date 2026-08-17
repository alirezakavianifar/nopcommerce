using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.AmazingDiscounts.Domain;
using Nop.Plugin.Misc.AmazingDiscounts.Models;
using Nop.Plugin.Misc.AmazingDiscounts.Services;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.AmazingDiscounts.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[Route("Admin/AmazingDiscounts")]
[AutoValidateAntiforgeryToken]
public class AmazingDiscountAdminController : BasePluginController
{
    protected readonly IAmazingDiscountService _amazingDiscountService;
    protected readonly IProductService _productService;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IPermissionService _permissionService;

    public AmazingDiscountAdminController(IAmazingDiscountService amazingDiscountService,
        IProductService productService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService)
    {
        _amazingDiscountService = amazingDiscountService;
        _productService = productService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
    }

    [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    [Route("List")]
    public virtual async Task<IActionResult> List()
    {
        var model = new AmazingDiscountSearchModel();
        return View("~/Plugins/Misc.AmazingDiscounts/Views/Admin/List.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    [Route("List")]
    public virtual async Task<IActionResult> List(AmazingDiscountSearchModel searchModel)
    {
        var products = await _amazingDiscountService.GetAllAmazingDiscountProductsAsync(searchModel.Page - 1, searchModel.PageSize);
        var model = new AmazingDiscountListModel().PrepareToGrid(searchModel, products, () =>
        {
            return products.Select(async p => {
                var product = await _productService.GetProductByIdAsync(p.ProductId);
                return new AmazingDiscountProductModel
                {
                    Id = p.Id,
                    ProductId = p.ProductId,
                    ProductName = product?.Name,
                    DisplayOrder = p.DisplayOrder,
                    CustomLabel = p.CustomLabel,
                    StartDateUtc = p.StartDateUtc,
                    EndDateUtc = p.EndDateUtc
                };
            }).Select(t => t.Result);
        });

        return Json(model);
    }

    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    [Route("Create")]
    public virtual async Task<IActionResult> Create()
    {
        var model = new AmazingDiscountProductModel();
        await PrepareProductListAsync(model);
        return View("~/Plugins/Misc.AmazingDiscounts/Views/Admin/Create.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    [Route("Create")]
    public virtual async Task<IActionResult> Create(AmazingDiscountProductModel model, bool continueEditing)
    {
        if (ModelState.IsValid && model.ProductId > 0)
        {
            var product = new AmazingDiscountProduct
            {
                ProductId = model.ProductId,
                DisplayOrder = model.DisplayOrder,
                CustomLabel = model.CustomLabel,
                StartDateUtc = model.StartDateUtc,
                EndDateUtc = model.EndDateUtc
            };

            await _amazingDiscountService.InsertAsync(product);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Common.Added"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = product.Id });
        }

        await PrepareProductListAsync(model);
        return View("~/Plugins/Misc.AmazingDiscounts/Views/Admin/Create.cshtml", model);
    }

    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    [Route("Edit/{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        var product = await _amazingDiscountService.GetByIdAsync(id);
        if (product == null)
            return RedirectToAction("List");

        var nopProduct = await _productService.GetProductByIdAsync(product.ProductId);
        var model = new AmazingDiscountProductModel
        {
            Id = product.Id,
            ProductId = product.ProductId,
            ProductName = nopProduct?.Name,
            DisplayOrder = product.DisplayOrder,
            CustomLabel = product.CustomLabel,
            StartDateUtc = product.StartDateUtc,
            EndDateUtc = product.EndDateUtc
        };

        await PrepareProductListAsync(model);
        return View("~/Plugins/Misc.AmazingDiscounts/Views/Admin/Edit.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    [Route("Edit/{id}")]
    public virtual async Task<IActionResult> Edit(AmazingDiscountProductModel model, bool continueEditing)
    {
        var product = await _amazingDiscountService.GetByIdAsync(model.Id);
        if (product == null)
            return RedirectToAction("List");

        if (ModelState.IsValid && model.ProductId > 0)
        {
            product.ProductId = model.ProductId;
            product.DisplayOrder = model.DisplayOrder;
            product.CustomLabel = model.CustomLabel;
            product.StartDateUtc = model.StartDateUtc;
            product.EndDateUtc = model.EndDateUtc;

            await _amazingDiscountService.UpdateAsync(product);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Common.Updated"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = product.Id });
        }

        await PrepareProductListAsync(model);
        return View("~/Plugins/Misc.AmazingDiscounts/Views/Admin/Edit.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    [Route("Delete/{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var product = await _amazingDiscountService.GetByIdAsync(id);
        if (product == null)
            return RedirectToAction("List");

        await _amazingDiscountService.DeleteAsync(product);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Common.Deleted"));

        return RedirectToAction("List");
    }

    protected virtual async Task PrepareProductListAsync(AmazingDiscountProductModel model)
    {
        var products = await _productService.SearchProductsAsync(pageIndex: 0, pageSize: 500);
        model.AvailableProducts = products.Select(p => new SelectListItem
        {
            Text = $"{p.Name} (ID: {p.Id})",
            Value = p.Id.ToString(),
            Selected = p.Id == model.ProductId
        }).ToList();

        model.AvailableProducts.Insert(0, new SelectListItem
        {
            Text = "--- یک محصول انتخاب کنید ---",
            Value = "0"
        });
    }
}
