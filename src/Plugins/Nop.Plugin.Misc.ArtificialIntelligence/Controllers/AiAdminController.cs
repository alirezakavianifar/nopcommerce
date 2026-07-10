using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Plugin.Misc.ArtificialIntelligence.Domain;
using Nop.Plugin.Misc.ArtificialIntelligence.Models;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class AiAdminController : BasePluginController
{
    private readonly ISettingService _settingService;
    private readonly IRepository<AiDuplicateProductQueue> _duplicateQueueRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IProductService _productService;
    private readonly IVendorService _vendorService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;

    public AiAdminController(
        ISettingService settingService,
        IRepository<AiDuplicateProductQueue> duplicateQueueRepository,
        IRepository<Product> productRepository,
        IProductService productService,
        IVendorService vendorService,
        INotificationService notificationService,
        ILocalizationService localizationService)
    {
        _settingService = settingService;
        _duplicateQueueRepository = duplicateQueueRepository;
        _productRepository = productRepository;
        _productService = productService;
        _vendorService = vendorService;
        _notificationService = notificationService;
        _localizationService = localizationService;
    }

    [HttpGet]
    public async Task<IActionResult> Configure()
    {
        var settings = await _settingService.LoadSettingAsync<AiSettings>();
        var model = new AiSettingsModel
        {
            ApiKey = settings.ApiKey,
            BaseUrl = settings.BaseUrl,
            SandboxMode = settings.SandboxMode,
            ChatbotModel = settings.ChatbotModel,
            VisionModel = settings.VisionModel,
            EmbeddingModel = settings.EmbeddingModel,
            DuplicateSimilarityThreshold = settings.DuplicateSimilarityThreshold
        };

        return View("~/Plugins/Misc.ArtificialIntelligence/Views/Admin/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(AiSettingsModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        var settings = await _settingService.LoadSettingAsync<AiSettings>();
        settings.ApiKey = model.ApiKey;
        settings.BaseUrl = model.BaseUrl;
        settings.SandboxMode = model.SandboxMode;
        settings.ChatbotModel = model.ChatbotModel;
        settings.VisionModel = model.VisionModel;
        settings.EmbeddingModel = model.EmbeddingModel;
        settings.DuplicateSimilarityThreshold = model.DuplicateSimilarityThreshold;

        await _settingService.SaveSettingAsync(settings);
        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Common.Updated"));

        return await Configure();
    }

    [HttpGet]
    public async Task<IActionResult> DuplicateQueueList()
    {
        var queueItems = await _duplicateQueueRepository.GetAllAsync(query => query.OrderByDescending(q => q.CreatedOnUtc));
        var modelList = new List<AiDuplicateProductModel>();

        foreach (var item in queueItems)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            var duplicateProduct = await _productRepository.GetByIdAsync(item.DuplicateProductId);
            var vendorName = "Global Store";
            if (item.VendorId > 0)
            {
                var vendor = await _vendorService.GetVendorByIdAsync(item.VendorId);
                if (vendor != null)
                {
                    vendorName = vendor.Name;
                }
            }

            modelList.Add(new AiDuplicateProductModel
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = product?.Name ?? "Deleted Product",
                ProductSku = product?.Sku ?? "N/A",
                DuplicateProductId = item.DuplicateProductId,
                DuplicateProductName = duplicateProduct?.Name ?? "Deleted Product",
                VendorId = item.VendorId,
                VendorName = vendorName,
                StatusId = item.StatusId,
                Status = item.Status.ToString(),
                Explanation = item.Explanation,
                CreatedOnUtc = item.CreatedOnUtc
            });
        }

        return View("~/Plugins/Misc.ArtificialIntelligence/Views/Admin/DuplicateQueueList.cshtml", modelList);
    }

    [HttpPost]
    public async Task<IActionResult> ApproveAsNew(int id)
    {
        var queueItem = await _duplicateQueueRepository.GetByIdAsync(id);
        if (queueItem == null)
            return NotFound();

        queueItem.Status = DuplicateStatus.ApprovedAsNew;
        queueItem.UpdatedOnUtc = DateTime.UtcNow;
        await _duplicateQueueRepository.UpdateAsync(queueItem);

        var product = await _productRepository.GetByIdAsync(queueItem.ProductId);
        if (product != null)
        {
            product.Published = true;
            await _productRepository.UpdateAsync(product);
        }

        _notificationService.SuccessNotification("Product approved and published successfully.");
        return RedirectToAction("DuplicateQueueList");
    }

    [HttpPost]
    public async Task<IActionResult> RejectBlock(int id)
    {
        var queueItem = await _duplicateQueueRepository.GetByIdAsync(id);
        if (queueItem == null)
            return NotFound();

        queueItem.Status = DuplicateStatus.Rejected;
        queueItem.UpdatedOnUtc = DateTime.UtcNow;
        await _duplicateQueueRepository.UpdateAsync(queueItem);

        var product = await _productRepository.GetByIdAsync(queueItem.ProductId);
        if (product != null)
        {
            product.Published = false;
            await _productRepository.UpdateAsync(product);
        }

        _notificationService.SuccessNotification("Product duplicate block confirmed.");
        return RedirectToAction("DuplicateQueueList");
    }
}
