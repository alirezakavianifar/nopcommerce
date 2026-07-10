using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Seo;
using Nop.Web.Framework.Controllers;
using Nop.Plugin.Misc.ArtificialIntelligence.Services;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Controllers;

public class AiStorefrontController : BasePluginController
{
    private readonly IAiService _aiService;
    private readonly IProductService _productService;
    private readonly IPictureService _pictureService;
    private readonly IPriceCalculationService _priceCalculationService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IEmailSender _emailSender;
    private readonly IEmailAccountService _emailAccountService;
    private readonly IWebHelper _webHelper;

    public AiStorefrontController(
        IAiService aiService,
        IProductService productService,
        IPictureService pictureService,
        IPriceCalculationService priceCalculationService,
        IPriceFormatter priceFormatter,
        IWorkContext workContext,
        IStoreContext storeContext,
        IUrlRecordService urlRecordService,
        IEmailSender emailSender,
        IEmailAccountService emailAccountService,
        IWebHelper webHelper)
    {
        _aiService = aiService;
        _productService = productService;
        _pictureService = pictureService;
        _priceCalculationService = priceCalculationService;
        _priceFormatter = priceFormatter;
        _workContext = workContext;
        _storeContext = storeContext;
        _urlRecordService = urlRecordService;
        _emailSender = emailSender;
        _emailAccountService = emailAccountService;
        _webHelper = webHelper;
    }

    [HttpPost]
    public async Task<IActionResult> VisualSearch(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded." });

        try
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            var productIds = await _aiService.VisualSearchAsync(fileBytes);
            var products = await _productService.GetProductsByIdsAsync(productIds.ToArray());
            var results = await MapProductsToJsonAsync(products);

            return Json(new { success = true, products = results });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> VoiceSearch(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded." });

        try
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            var queryText = await _aiService.SpeechToTextAsync(fileBytes, file.FileName);
            if (string.IsNullOrWhiteSpace(queryText))
            {
                return Json(new { success = true, query = "", products = new List<object>() });
            }

            var searchResults = await _productService.SearchProductsAsync(keywords: queryText);
            var results = await MapProductsToJsonAsync(searchResults.Take(6).ToList());

            return Json(new { success = true, query = queryText, products = results });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ChatbotSendMessage([FromBody] List<ChatMessageModel> history)
    {
        if (history == null)
            return Json(new { success = false, message = "Empty chat history." });

        try
        {
            var messages = new List<object>();
            
            messages.Add(new
            {
                role = "system",
                content = "You are a helpful customer service assistant for our online multi-vendor marketplace. Answer in the customer's language (Persian or English). Be polite, helpful, and concise. Refer user to customer support if you cannot answer."
            });

            foreach (var h in history)
            {
                messages.Add(new { role = h.Role, content = h.Content });
            }

            var reply = await _aiService.ChatResponseAsync(messages);

            var triggerHandoff = reply.Contains("ارجاع به پشتیبان") || 
                                 reply.Contains("contact support") || 
                                 reply.Contains("human agent");

            return Json(new { success = true, reply = reply, triggerHandoff = triggerHandoff });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ChatbotHandoff(string name, string email, string message)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            return Json(new { success = false, message = "All fields are required." });

        try
        {
            var emailAccount = (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
            if (emailAccount != null)
            {
                var subject = $"AI Chatbot Handoff Request from {name}";
                var body = $"<h3>AI Chatbot Handoff Contact Request</h3>" +
                           $"<p><b>Name:</b> {name}</p>" +
                           $"<p><b>Email:</b> {email}</p>" +
                           $"<p><b>Query/Description:</b> {message}</p>";

                await _emailSender.SendEmailAsync(
                    emailAccount,
                    subject,
                    body,
                    emailAccount.Email,
                    emailAccount.DisplayName,
                    email,
                    name
                );

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "No configured email account found." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    #region Helpers

    private async Task<List<object>> MapProductsToJsonAsync(IList<Product> products)
    {
        var results = new List<object>();
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        foreach (var product in products)
        {
            var seName = await _urlRecordService.GetSeNameAsync(product);
            var picture = await _pictureService.GetProductPictureAsync(product, null);
            var pictureUrl = picture != null 
                ? (await _pictureService.GetPictureUrlAsync(picture, 150)).Url 
                : await _pictureService.GetDefaultPictureUrlAsync(150);

            var (_, finalPrice, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, store);
            var priceStr = await _priceFormatter.FormatPriceAsync(finalPrice);

            results.Add(new
            {
                id = product.Id,
                name = product.Name,
                url = $"{_webHelper.GetStoreLocation()}{seName}",
                pictureUrl = pictureUrl,
                price = priceStr
            });
        }

        return results;
    }

    #endregion
}

public class ChatMessageModel
{
    public string Role { get; set; }
    public string Content { get; set; }
}
