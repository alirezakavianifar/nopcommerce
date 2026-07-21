using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Nop.Plugin.Misc.ArtificialIntelligence.Services;

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
    private readonly IAvalAiClient _avalAiClient;

    public AiAdminController(
        ISettingService settingService,
        IRepository<AiDuplicateProductQueue> duplicateQueueRepository,
        IRepository<Product> productRepository,
        IProductService productService,
        IVendorService vendorService,
        INotificationService notificationService,
        ILocalizationService localizationService,
        IAvalAiClient avalAiClient)
    {
        _settingService = settingService;
        _duplicateQueueRepository = duplicateQueueRepository;
        _productRepository = productRepository;
        _productService = productService;
        _vendorService = vendorService;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _avalAiClient = avalAiClient;
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
            ProviderTypeId = (int)settings.ProviderType,
            LocalSttEndpoint = settings.LocalSttEndpoint,
            LocalEmbeddingEndpoint = settings.LocalEmbeddingEndpoint,
            LocalVisionEndpoint = settings.LocalVisionEndpoint,
            LocalChatEndpoint = settings.LocalChatEndpoint,
            LocalApiKey = settings.LocalApiKey,
            LocalSttModel = settings.LocalSttModel,
            LocalEmbeddingModel = settings.LocalEmbeddingModel,
            LocalVisionModel = settings.LocalVisionModel,
            LocalChatModel = settings.LocalChatModel,
            EnableClientWebSpeechFallback = settings.EnableClientWebSpeechFallback,
            ChatbotModel = settings.ChatbotModel,
            VisionModel = settings.VisionModel,
            EmbeddingModel = settings.EmbeddingModel,
            DuplicateSimilarityThreshold = settings.DuplicateSimilarityThreshold,
            CreditThreshold = settings.CreditThreshold
        };

        decimal totalRemainingIrt = 0;
        bool hasCreditInfo = false;

        if (settings.SandboxMode)
        {
            totalRemainingIrt = 125000m; // Mock balance
            hasCreditInfo = true;
        }
        else if (!string.IsNullOrEmpty(settings.ApiKey))
        {
            var creditInfo = await _avalAiClient.GetCreditAsync(settings.ApiKey, settings.BaseUrl);
            if (creditInfo != null)
            {
                totalRemainingIrt = creditInfo.RemainingIrt;
                if (creditInfo.CreditSources?.Grants != null)
                {
                    foreach (var grant in creditInfo.CreditSources.Grants)
                    {
                        if (decimal.TryParse(grant.RemainingIrt, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var grantVal))
                        {
                            totalRemainingIrt += grantVal;
                        }
                    }
                }
                if (creditInfo.CreditSources?.Packages != null)
                {
                    foreach (var package in creditInfo.CreditSources.Packages)
                    {
                        if (decimal.TryParse(package.RemainingIrt, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var pkgVal))
                        {
                            totalRemainingIrt += pkgVal;
                        }
                    }
                }
                hasCreditInfo = true;
            }
            else
            {
                var warningFormat = await _localizationService.GetResourceAsync("Plugins.Misc.ArtificialIntelligence.CreditFetchError");
                if (string.IsNullOrEmpty(warningFormat) || warningFormat.Equals("Plugins.Misc.ArtificialIntelligence.CreditFetchError"))
                {
                    warningFormat = "Could not retrieve AvalAI credit information. Please verify your API Key and connection.";
                }
                _notificationService.WarningNotification(warningFormat);
            }
        }

        if (hasCreditInfo)
        {
            model.CurrentCredit = totalRemainingIrt;

            if (totalRemainingIrt <= settings.CreditThreshold)
            {
                var warningFormat = await _localizationService.GetResourceAsync("Plugins.Misc.ArtificialIntelligence.CreditWarning");
                if (string.IsNullOrEmpty(warningFormat) || warningFormat.Equals("Plugins.Misc.ArtificialIntelligence.CreditWarning"))
                {
                    warningFormat = "AvalAI credit is low. Remaining credit is {0} Tomans, which is below the threshold of {1} Tomans.";
                }

                var text = string.Format(warningFormat, totalRemainingIrt.ToString("N0"), settings.CreditThreshold.ToString("N0"));
                if (settings.SandboxMode)
                {
                    text = $"[Sandbox] {text}";
                }
                _notificationService.WarningNotification(text);
            }
        }

        // Fetch and map available models
        var rawModels = await _avalAiClient.GetModelsAsync(settings.ApiKey, settings.BaseUrl);
        
        var chatbotModels = rawModels
            .Where(m => m.Mode != null && (m.Mode.Contains("chat") || m.Mode.Contains("completion") || m.SupportsVision) && !m.Mode.Contains("embedding"))
            .OrderBy(m => m.InputPrice)
            .Select(m => new AvalAiModelDto
            {
                Value = m.Id,
                Text = $"{m.Id} ({m.OwnedBy})",
                InputPrice = m.InputPrice.ToString("F3"),
                OutputPrice = m.OutputPrice.ToString("F3"),
                Provider = m.OwnedBy,
                SupportsVision = m.SupportsVision.ToString()
            }).ToList();

        var visionModels = rawModels
            .Where(m => m.SupportsVision)
            .OrderBy(m => m.InputPrice)
            .Select(m => new AvalAiModelDto
            {
                Value = m.Id,
                Text = $"{m.Id} ({m.OwnedBy})",
                InputPrice = m.InputPrice.ToString("F3"),
                OutputPrice = m.OutputPrice.ToString("F3"),
                Provider = m.OwnedBy,
                SupportsVision = m.SupportsVision.ToString()
            }).ToList();

        var embeddingModels = rawModels
            .Where(m => m.Mode != null && m.Mode.Contains("embedding"))
            .OrderBy(m => m.InputPrice)
            .Select(m => new AvalAiModelDto
            {
                Value = m.Id,
                Text = $"{m.Id} ({m.OwnedBy})",
                InputPrice = m.InputPrice.ToString("F3"),
                OutputPrice = m.OutputPrice.ToString("F3"),
                Provider = m.OwnedBy,
                SupportsVision = m.SupportsVision.ToString()
            }).ToList();

        if (!chatbotModels.Any(m => m.Value == settings.ChatbotModel) && !string.IsNullOrEmpty(settings.ChatbotModel))
        {
            chatbotModels.Insert(0, new AvalAiModelDto
            {
                Value = settings.ChatbotModel,
                Text = $"{settings.ChatbotModel} (Configured)",
                InputPrice = "0.000",
                OutputPrice = "0.000",
                Provider = "configured",
                SupportsVision = "True"
            });
        }

        if (!visionModels.Any(m => m.Value == settings.VisionModel) && !string.IsNullOrEmpty(settings.VisionModel))
        {
            visionModels.Insert(0, new AvalAiModelDto
            {
                Value = settings.VisionModel,
                Text = $"{settings.VisionModel} (Configured)",
                InputPrice = "0.000",
                OutputPrice = "0.000",
                Provider = "configured",
                SupportsVision = "True"
            });
        }

        if (!embeddingModels.Any(m => m.Value == settings.EmbeddingModel) && !string.IsNullOrEmpty(settings.EmbeddingModel))
        {
            embeddingModels.Insert(0, new AvalAiModelDto
            {
                Value = settings.EmbeddingModel,
                Text = $"{settings.EmbeddingModel} (Configured)",
                InputPrice = "0.000",
                OutputPrice = "0.000",
                Provider = "configured",
                SupportsVision = "False"
            });
        }

        model.AvailableChatbotModels = chatbotModels;
        model.AvailableVisionModels = visionModels;
        model.AvailableEmbeddingModels = embeddingModels;

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
        settings.ProviderType = (AiProviderType)model.ProviderTypeId;
        settings.LocalSttEndpoint = model.LocalSttEndpoint;
        settings.LocalEmbeddingEndpoint = model.LocalEmbeddingEndpoint;
        settings.LocalVisionEndpoint = model.LocalVisionEndpoint;
        settings.LocalChatEndpoint = model.LocalChatEndpoint;
        settings.LocalApiKey = model.LocalApiKey;
        settings.LocalSttModel = model.LocalSttModel;
        settings.LocalEmbeddingModel = model.LocalEmbeddingModel;
        settings.LocalVisionModel = model.LocalVisionModel;
        settings.LocalChatModel = model.LocalChatModel;
        settings.EnableClientWebSpeechFallback = model.EnableClientWebSpeechFallback;
        settings.ChatbotModel = model.ChatbotModel;
        settings.VisionModel = model.VisionModel;
        settings.EmbeddingModel = model.EmbeddingModel;
        settings.DuplicateSimilarityThreshold = model.DuplicateSimilarityThreshold;
        settings.CreditThreshold = model.CreditThreshold;

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
