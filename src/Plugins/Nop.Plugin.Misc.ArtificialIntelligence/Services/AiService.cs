using System.Text.Json;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Plugin.Misc.ArtificialIntelligence.Domain;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Services;

public class AiService : IAiService
{
    private readonly IAvalAiClient _avalAiClient;
    private readonly ISettingService _settingService;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<ProductEmbeddingCache> _embeddingCacheRepository;
    private readonly IProductService _productService;
    private readonly ILogger _logger;

    public AiService(
        IAvalAiClient avalAiClient,
        ISettingService settingService,
        IRepository<Product> productRepository,
        IRepository<ProductEmbeddingCache> embeddingCacheRepository,
        IProductService productService,
        ILogger logger)
    {
        _avalAiClient = avalAiClient;
        _settingService = settingService;
        _productRepository = productRepository;
        _embeddingCacheRepository = embeddingCacheRepository;
        _productService = productService;
        _logger = logger;
    }

    private async Task<AiSettings> GetSettingsAsync()
    {
        return await _settingService.LoadSettingAsync<AiSettings>();
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var settings = await GetSettingsAsync();
        if (settings.SandboxMode)
        {
            return GetSandboxEmbedding(text);
        }

        if (string.IsNullOrEmpty(settings.ApiKey))
        {
            await _logger.WarningAsync("AvalAI API Key is missing. Falling back to sandbox embeddings.");
            return GetSandboxEmbedding(text);
        }

        return await _avalAiClient.GetEmbeddingAsync(text, settings.ApiKey, settings.EmbeddingModel, settings.BaseUrl);
    }

    public async Task<string> SpeechToTextAsync(byte[] audioData, string filename)
    {
        var settings = await GetSettingsAsync();
        if (settings.SandboxMode)
        {
            // Simulated transcription based on input length or common Persian store searches
            var keywords = new[] { "کفش ورزشی", "گوشی موبایل", "تیشرت مردانه", "لوازم آشپزخانه", "کتاب آموزش آشپزی" };
            var rnd = new Random(audioData.Length);
            return keywords[rnd.Next(keywords.Length)];
        }

        if (string.IsNullOrEmpty(settings.ApiKey))
        {
            await _logger.WarningAsync("AvalAI API Key is missing. SpeechToText skipped.");
            return "کفش ورزشی (شبیه‌سازی‌شده)";
        }

        return await _avalAiClient.SpeechToTextAsync(audioData, filename, settings.ApiKey, settings.BaseUrl);
    }

    public async Task<string> ChatResponseAsync(IList<object> history)
    {
        var settings = await GetSettingsAsync();
        if (settings.SandboxMode)
        {
            // Simulated Persian/English response
            return "سلام! من پشتیبان هوشمند شما هستم. چطور می‌توانم در مورد محصولات یا روند خرید به شما کمک کنم؟ اگر سوال شما برطرف نشد، در هر زمان بنویسید 'ارجاع به پشتیبان' تا به پشتیبانی فیزیکی متصل شوید.";
        }

        if (string.IsNullOrEmpty(settings.ApiKey))
        {
            return "در حال حاضر ارتباط با سرور هوش مصنوعی برقرار نیست (کلید تنظیم نشده است).";
        }

        return await _avalAiClient.GetChatResponseAsync(history, settings.ApiKey, settings.ChatbotModel, settings.BaseUrl);
    }

    public async Task<IList<int>> VisualSearchAsync(byte[] imageData)
    {
        var settings = await GetSettingsAsync();
        if (settings.SandboxMode)
        {
            // Return top 3 products from repository as mockup results
            var allProducts = await _productRepository.GetAllAsync(query => query.Take(3));
            return allProducts.Select(p => p.Id).ToList();
        }

        if (string.IsNullOrEmpty(settings.ApiKey))
        {
            var allProducts = await _productRepository.GetAllAsync(query => query.Take(2));
            return allProducts.Select(p => p.Id).ToList();
        }

        // Call vision model to get tags, then search database by keywords
        var prompt = "What is the product in this image? Provide exactly 3 search keywords separated by commas in Persian language. Do not output anything else.";
        var keywordsCsv = await _avalAiClient.AnalyzeImageAsync(imageData, prompt, settings.ApiKey, settings.VisionModel, settings.BaseUrl);
        
        if (string.IsNullOrEmpty(keywordsCsv))
            return new List<int>();

        var keywords = keywordsCsv.Split(',').Select(k => k.Trim()).ToList();
        var matches = new List<int>();
        foreach (var keyword in keywords)
        {
            var searchResults = await _productService.SearchProductsAsync(keywords: keyword);
            matches.AddRange(searchResults.Select(p => p.Id));
        }

        return matches.Distinct().Take(6).ToList();
    }

    public async Task<AiDuplicateCheckResult> CheckDuplicateAsync(int productId)
    {
        var settings = await GetSettingsAsync();
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            return new AiDuplicateCheckResult { IsDuplicate = false };
        }

        var sourceText = $"{product.Name} {product.FullDescription}";
        var sourceVector = await GetEmbeddingAsync(sourceText);

        if (sourceVector == null || sourceVector.Length == 0)
        {
            return new AiDuplicateCheckResult { IsDuplicate = false };
        }

        // Cache the embedding for the new product
        var cacheEntry = new ProductEmbeddingCache
        {
            ProductId = product.Id,
            VectorJson = JsonSerializer.Serialize(sourceVector),
            LastUpdatedOnUtc = DateTime.UtcNow
        };
        await _embeddingCacheRepository.InsertAsync(cacheEntry);

        // Fetch existing cached embeddings for comparison (exclude current product)
        var allCaches = await _embeddingCacheRepository.GetAllAsync(query => query.Where(c => c.ProductId != productId));
        
        foreach (var cache in allCaches)
        {
            var targetVector = JsonSerializer.Deserialize<float[]>(cache.VectorJson);
            if (targetVector == null || targetVector.Length == 0)
                continue;

            var similarity = (decimal)CosineSimilarity(sourceVector, targetVector);
            if (similarity >= settings.DuplicateSimilarityThreshold)
            {
                return new AiDuplicateCheckResult
                {
                    IsDuplicate = true,
                    DuplicateProductId = cache.ProductId,
                    Confidence = similarity
                };
            }
        }

        return new AiDuplicateCheckResult { IsDuplicate = false };
    }

    #region Helpers

    private float[] GetSandboxEmbedding(string text)
    {
        var hash = text.GetHashCode();
        var rnd = new Random(hash);
        var vector = new float[1536];
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i] = (float)(rnd.NextDouble() * 2.0 - 1.0);
        }
        // Normalize
        double sum = 0.0;
        for (int i = 0; i < vector.Length; i++)
            sum += vector[i] * vector[i];
        
        double norm = Math.Sqrt(sum);
        if (norm > 0)
        {
            for (int i = 0; i < vector.Length; i++)
                vector[i] = (float)(vector[i] / norm);
        }
        return vector;
    }

    private double CosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA == null || vectorB == null || vectorA.Length == 0 || vectorB.Length == 0 || vectorA.Length != vectorB.Length)
            return 0;

        double dotProduct = 0.0;
        double normA = 0.0;
        double normB = 0.0;
        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            normA += vectorA[i] * vectorA[i];
            normB += vectorB[i] * vectorB[i];
        }

        if (normA == 0.0 || normB == 0.0)
            return 0;

        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    #endregion
}
