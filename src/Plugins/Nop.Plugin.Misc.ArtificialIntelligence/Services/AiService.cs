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
    private readonly IAiProviderFactory _aiProviderFactory;
    private readonly ISettingService _settingService;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<ProductEmbeddingCache> _embeddingCacheRepository;
    private readonly IProductService _productService;
    private readonly ILogger _logger;

    public AiService(
        IAiProviderFactory aiProviderFactory,
        ISettingService settingService,
        IRepository<Product> productRepository,
        IRepository<ProductEmbeddingCache> embeddingCacheRepository,
        IProductService productService,
        ILogger logger)
    {
        _aiProviderFactory = aiProviderFactory;
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
        if (settings.SandboxMode || settings.ProviderType == AiProviderType.Sandbox)
        {
            return GetSandboxEmbedding(text);
        }

        if (settings.ProviderType == AiProviderType.CloudAvalAi && string.IsNullOrEmpty(settings.ApiKey))
        {
            await _logger.WarningAsync("AvalAI API Key is missing. Falling back to sandbox embeddings.");
            return GetSandboxEmbedding(text);
        }

        var client = _aiProviderFactory.GetClient(settings);
        return await client.GetEmbeddingAsync(text, settings);
    }

    public async Task<string> SpeechToTextAsync(byte[] audioData, string filename)
    {
        var settings = await GetSettingsAsync();
        if (settings.SandboxMode || settings.ProviderType == AiProviderType.Sandbox)
        {
            // Simulated transcription based on input length or common Persian store searches
            var keywords = new[] { "کفش ورزشی", "گوشی موبایل", "تیشرت مردانه", "لوازم آشپزخانه", "کتاب آموزش آشپزی" };
            var rnd = new Random(audioData.Length);
            return keywords[rnd.Next(keywords.Length)];
        }

        if (settings.ProviderType == AiProviderType.CloudAvalAi && string.IsNullOrEmpty(settings.ApiKey))
        {
            await _logger.WarningAsync("AvalAI API Key is missing. SpeechToText skipped.");
            return "کفش ورزشی (شبیه‌سازی‌شده)";
        }

        var client = _aiProviderFactory.GetClient(settings);
        return await client.SpeechToTextAsync(audioData, filename, settings);
    }

    public async Task<string> ChatResponseAsync(IList<object> history)
    {
        var settings = await GetSettingsAsync();
        if (settings.SandboxMode || settings.ProviderType == AiProviderType.Sandbox)
        {
            // Simulated Persian/English response
            return "سلام! من پشتیبان هوشمند شما هستم. چطور می‌توانم در مورد محصولات یا روند خرید به شما کمک کنم؟ اگر سوال شما برطرف نشد، در هر زمان بنویسید 'ارجاع به پشتیبان' تا به پشتیبانی فیزیکی متصل شوید.";
        }

        if (settings.ProviderType == AiProviderType.CloudAvalAi && string.IsNullOrEmpty(settings.ApiKey))
        {
            return "در حال حاضر ارتباط با سرور هوش مصنوعی برقرار نیست (کلید تنظیم نشده است).";
        }

        var client = _aiProviderFactory.GetClient(settings);
        return await client.GetChatResponseAsync(history, settings);
    }

    public async Task<IList<int>> VisualSearchAsync(byte[] imageData)
    {
        var settings = await GetSettingsAsync();
        if (settings.SandboxMode || settings.ProviderType == AiProviderType.Sandbox)
        {
            // Return top 3 products from repository as mockup results
            var allProducts = await _productRepository.GetAllAsync(query => query.Take(3));
            return allProducts.Select(p => p.Id).ToList();
        }

        if (settings.ProviderType == AiProviderType.CloudAvalAi && string.IsNullOrEmpty(settings.ApiKey))
        {
            var allProducts = await _productRepository.GetAllAsync(query => query.Take(2));
            return allProducts.Select(p => p.Id).ToList();
        }

        var client = _aiProviderFactory.GetClient(settings);
        // Call vision model to get tags, then search database by keywords
        var prompt = "What is the product in this image? Provide exactly 3 search keywords separated by commas in Persian language. Do not output anything else.";
        var keywordsCsv = await client.AnalyzeImageAsync(imageData, prompt, settings);
        
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
        var sourceHash = GetMd5Hash(sourceText);

        // Fetch existing cache entry for this product if it exists
        var existingCache = (await _embeddingCacheRepository.GetAllAsync(query =>
            query.Where(c => c.ProductId == productId)
        )).FirstOrDefault();

        float[] sourceVector = null;
        bool cacheMatched = false;

        if (existingCache != null)
        {
            try
            {
                var trimmedJson = existingCache.VectorJson.Trim();
                if (trimmedJson.StartsWith("{"))
                {
                    var cachedData = JsonSerializer.Deserialize<ProductEmbeddingData>(existingCache.VectorJson);
                    if (cachedData != null && cachedData.Vector != null && cachedData.Vector.Length > 0)
                    {
                        if (cachedData.Hash == sourceHash)
                        {
                            sourceVector = cachedData.Vector;
                            cacheMatched = true;
                        }
                    }
                }
                else if (trimmedJson.StartsWith("["))
                {
                    // Old format: just the vector. We reuse it, but we don't have a hash to match.
                    // For backward-compatibility and token saving, we assume it matches, but we will update it later to save the hash.
                    sourceVector = JsonSerializer.Deserialize<float[]>(existingCache.VectorJson);
                    cacheMatched = true;
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Failed to deserialize embedding cache for product {productId}", ex);
            }
        }

        if (sourceVector == null || sourceVector.Length == 0)
        {
            sourceVector = await GetEmbeddingAsync(sourceText);
            if (sourceVector == null || sourceVector.Length == 0)
            {
                return new AiDuplicateCheckResult { IsDuplicate = false };
            }
            cacheMatched = false;
        }

        // Cache the embedding if it's new or hash has changed/needs update
        if (!cacheMatched)
        {
            var serializedCache = JsonSerializer.Serialize(new ProductEmbeddingData
            {
                Vector = sourceVector,
                Hash = sourceHash
            });

            if (existingCache != null)
            {
                existingCache.VectorJson = serializedCache;
                existingCache.LastUpdatedOnUtc = DateTime.UtcNow;
                await _embeddingCacheRepository.UpdateAsync(existingCache);
            }
            else
            {
                var cacheEntry = new ProductEmbeddingCache
                {
                    ProductId = product.Id,
                    VectorJson = serializedCache,
                    LastUpdatedOnUtc = DateTime.UtcNow
                };
                await _embeddingCacheRepository.InsertAsync(cacheEntry);
            }
        }

        // Fetch existing cached embeddings for comparison (exclude current product)
        var allCaches = await _embeddingCacheRepository.GetAllAsync(query => query.Where(c => c.ProductId != productId));
        
        // Group by ProductId and pick the latest one to handle duplicate database rows gracefully
        var uniqueCaches = allCaches
            .GroupBy(c => c.ProductId)
            .Select(g => g.OrderByDescending(c => c.LastUpdatedOnUtc).First())
            .ToList();

        foreach (var cache in uniqueCaches)
        {
            float[] targetVector = null;
            try
            {
                var trimmedJson = cache.VectorJson.Trim();
                if (trimmedJson.StartsWith("{"))
                {
                    var cachedData = JsonSerializer.Deserialize<ProductEmbeddingData>(cache.VectorJson);
                    targetVector = cachedData?.Vector;
                }
                else if (trimmedJson.StartsWith("["))
                {
                    targetVector = JsonSerializer.Deserialize<float[]>(cache.VectorJson);
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Failed to deserialize embedding cache for target product {cache.ProductId}", ex);
            }

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

    public async Task<IList<int>> TextSearchAsync(string query, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<int>();

        var scoredMatches = new List<(int ProductId, double Score)>();

        try
        {
            var queryVector = await GetEmbeddingAsync(query);

            if (queryVector != null && queryVector.Length > 0)
            {
                var allCaches = await _embeddingCacheRepository.GetAllAsync(q => q);
                var uniqueCaches = allCaches
                    .GroupBy(c => c.ProductId)
                    .Select(g => g.OrderByDescending(c => c.LastUpdatedOnUtc).First())
                    .ToList();

                foreach (var cache in uniqueCaches)
                {
                    float[] targetVector = null;
                    try
                    {
                        var trimmedJson = cache.VectorJson.Trim();
                        if (trimmedJson.StartsWith("{"))
                        {
                            var cachedData = JsonSerializer.Deserialize<ProductEmbeddingData>(cache.VectorJson);
                            targetVector = cachedData?.Vector;
                        }
                        else if (trimmedJson.StartsWith("["))
                        {
                            targetVector = JsonSerializer.Deserialize<float[]>(cache.VectorJson);
                        }
                    }
                    catch (Exception ex)
                    {
                        await _logger.ErrorAsync($"Failed to deserialize embedding cache for product {cache.ProductId}", ex);
                    }

                    if (targetVector == null || targetVector.Length == 0)
                        continue;

                    var similarity = CosineSimilarity(queryVector, targetVector);
                    if (similarity > 0.05)
                    {
                        scoredMatches.Add((cache.ProductId, similarity));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Error performing semantic AI text search for query: {query}", ex);
        }

        var resultIds = scoredMatches
            .OrderByDescending(m => m.Score)
            .Select(m => m.ProductId)
            .Take(maxResults)
            .ToList();

        // Fallback / supplement with keyword search to guarantee high-quality results
        if (resultIds.Count < maxResults)
        {
            try
            {
                var keywordMatches = await _productService.SearchProductsAsync(keywords: query);
                foreach (var p in keywordMatches)
                {
                    if (!resultIds.Contains(p.Id))
                    {
                        resultIds.Add(p.Id);
                        if (resultIds.Count >= maxResults)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Keyword search fallback error in TextSearchAsync for query: {query}", ex);
            }
        }

        return resultIds;
    }

    #region Helpers

    private string GetMd5Hash(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        using var md5 = System.Security.Cryptography.MD5.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hashBytes = md5.ComputeHash(bytes);
        return string.Concat(hashBytes.Select(b => b.ToString("x2")));
    }

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

public class ProductEmbeddingData
{
    public float[] Vector { get; set; }
    public string Hash { get; set; }
}
