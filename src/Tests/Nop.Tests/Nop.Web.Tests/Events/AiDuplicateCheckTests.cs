using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Misc.ArtificialIntelligence.Domain;
using Nop.Plugin.Misc.ArtificialIntelligence.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using NUnit.Framework;

namespace Nop.Tests.Nop.Web.Tests.Events;

[TestFixture]
public class AiDuplicateCheckTests
{
    private Mock<IAvalAiClient> _mockAvalAiClient;
    private Mock<IAiProviderFactory> _mockProviderFactory;
    private Mock<ISettingService> _mockSettingService;
    private Mock<IRepository<Product>> _mockProductRepository;
    private Mock<IRepository<ProductEmbeddingCache>> _mockEmbeddingCacheRepository;
    private Mock<IProductService> _mockProductService;
    private Mock<ILogger> _mockLogger;
    private AiSettings _settings;
    private List<ProductEmbeddingCache> _cacheDatabase;

    [SetUp]
    public void SetUp()
    {
        _mockAvalAiClient = new Mock<IAvalAiClient>();
        _mockProviderFactory = new Mock<IAiProviderFactory>();
        _mockProviderFactory.Setup(f => f.GetClient(It.IsAny<AiSettings>())).Returns(_mockAvalAiClient.Object);
        _mockSettingService = new Mock<ISettingService>();
        _mockProductRepository = new Mock<IRepository<Product>>();
        _mockEmbeddingCacheRepository = new Mock<IRepository<ProductEmbeddingCache>>();
        _mockProductService = new Mock<IProductService>();
        _mockLogger = new Mock<ILogger>();

        _settings = new AiSettings
        {
            SandboxMode = false,
            ApiKey = "test-key",
            EmbeddingModel = "text-embedding-3-small",
            BaseUrl = "https://api.example.com",
            DuplicateSimilarityThreshold = 0.9m
        };

        _mockSettingService.Setup(s => s.LoadSettingAsync<AiSettings>(It.IsAny<int>()))
            .ReturnsAsync(_settings);

        _cacheDatabase = new List<ProductEmbeddingCache>();

        // Set up GetAllAsync to dynamically execute the query lambda against our in-memory _cacheDatabase
        _mockEmbeddingCacheRepository.Setup(r => r.GetAllAsync(
                It.IsAny<Func<IQueryable<ProductEmbeddingCache>, IQueryable<ProductEmbeddingCache>>>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((Func<IQueryable<ProductEmbeddingCache>, IQueryable<ProductEmbeddingCache>> func, Func<ICacheKeyService, CacheKey> key, bool incDeleted) =>
            {
                if (func == null) return _cacheDatabase;
                var query = _cacheDatabase.AsQueryable();
                return func(query).ToList();
            });
    }

    private string ComputeMd5(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hashBytes = md5.ComputeHash(bytes);
        return string.Concat(hashBytes.Select(b => b.ToString("x2")));
    }

    [Test]
    public async Task CheckDuplicateAsync_NoCache_CallsApiAndInsertsCache()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Laptop", FullDescription = "A great laptop" };
        _mockProductRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<Func<ICacheKeyService, CacheKey>>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(product);

        var expectedVector = new float[] { 0.1f, 0.2f, 0.3f };
        _mockAvalAiClient.Setup(c => c.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<AiSettings>()))
            .ReturnsAsync(expectedVector);

        var service = new AiService(
            _mockProviderFactory.Object,
            _mockSettingService.Object,
            _mockProductRepository.Object,
            _mockEmbeddingCacheRepository.Object,
            _mockProductService.Object,
            _mockLogger.Object
        );

        // Act
        var result = await service.CheckDuplicateAsync(product.Id);

        // Assert
        result.IsDuplicate.Should().BeFalse();
        _mockAvalAiClient.Verify(c => c.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<AiSettings>()), Times.Once);
        _mockEmbeddingCacheRepository.Verify(r => r.InsertAsync(It.IsAny<ProductEmbeddingCache>(), It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public async Task CheckDuplicateAsync_CacheExistsAndMatchesHash_ReusesVectorWithoutCallingApi()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Laptop", FullDescription = "A great laptop" };
        _mockProductRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<Func<ICacheKeyService, CacheKey>>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(product);

        var vector = new float[] { 0.1f, 0.2f, 0.3f };
        var cacheData = new ProductEmbeddingData
        {
            Vector = vector,
            Hash = ComputeMd5("Laptop A great laptop")
        };
        var cacheEntry = new ProductEmbeddingCache
        {
            ProductId = product.Id,
            VectorJson = JsonSerializer.Serialize(cacheData),
            LastUpdatedOnUtc = DateTime.UtcNow
        };

        _cacheDatabase.Add(cacheEntry);

        var service = new AiService(
            _mockProviderFactory.Object,
            _mockSettingService.Object,
            _mockProductRepository.Object,
            _mockEmbeddingCacheRepository.Object,
            _mockProductService.Object,
            _mockLogger.Object
        );

        // Act
        var result = await service.CheckDuplicateAsync(product.Id);

        // Assert
        result.IsDuplicate.Should().BeFalse();
        _mockAvalAiClient.Verify(c => c.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<AiSettings>()), Times.Never);
        _mockEmbeddingCacheRepository.Verify(r => r.InsertAsync(It.IsAny<ProductEmbeddingCache>(), It.IsAny<bool>()), Times.Never);
        _mockEmbeddingCacheRepository.Verify(r => r.UpdateAsync(It.IsAny<ProductEmbeddingCache>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task CheckDuplicateAsync_CacheExistsButHashDiffers_CallsApiAndUpdatesCache()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Laptop New", FullDescription = "A great laptop" };
        _mockProductRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<Func<ICacheKeyService, CacheKey>>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(product);

        var oldVector = new float[] { 0.1f, 0.2f, 0.3f };
        var cacheData = new ProductEmbeddingData
        {
            Vector = oldVector,
            Hash = "old-hash"
        };
        var cacheEntry = new ProductEmbeddingCache
        {
            ProductId = product.Id,
            VectorJson = JsonSerializer.Serialize(cacheData),
            LastUpdatedOnUtc = DateTime.UtcNow
        };

        _cacheDatabase.Add(cacheEntry);

        var newVector = new float[] { 0.4f, 0.5f, 0.6f };
        _mockAvalAiClient.Setup(c => c.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<AiSettings>()))
            .ReturnsAsync(newVector);

        var service = new AiService(
            _mockProviderFactory.Object,
            _mockSettingService.Object,
            _mockProductRepository.Object,
            _mockEmbeddingCacheRepository.Object,
            _mockProductService.Object,
            _mockLogger.Object
        );

        // Act
        var result = await service.CheckDuplicateAsync(product.Id);

        // Assert
        result.IsDuplicate.Should().BeFalse();
        _mockAvalAiClient.Verify(c => c.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<AiSettings>()), Times.Once);
        _mockEmbeddingCacheRepository.Verify(r => r.UpdateAsync(It.IsAny<ProductEmbeddingCache>(), It.IsAny<bool>()), Times.Once);
        _mockEmbeddingCacheRepository.Verify(r => r.InsertAsync(It.IsAny<ProductEmbeddingCache>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task CheckDuplicateAsync_OldCacheFormat_ReusesVectorAndSavesNewFormat()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Laptop", FullDescription = "A great laptop" };
        _mockProductRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<Func<ICacheKeyService, CacheKey>>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(product);

        var rawVector = new float[] { 0.1f, 0.2f, 0.3f };
        var cacheEntry = new ProductEmbeddingCache
        {
            ProductId = product.Id,
            VectorJson = JsonSerializer.Serialize(rawVector), // Old format: JSON array of floats
            LastUpdatedOnUtc = DateTime.UtcNow
        };

        _cacheDatabase.Add(cacheEntry);

        var service = new AiService(
            _mockProviderFactory.Object,
            _mockSettingService.Object,
            _mockProductRepository.Object,
            _mockEmbeddingCacheRepository.Object,
            _mockProductService.Object,
            _mockLogger.Object
        );

        // Act
        var result = await service.CheckDuplicateAsync(product.Id);

        // Assert
        result.IsDuplicate.Should().BeFalse();
        _mockAvalAiClient.Verify(c => c.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<AiSettings>()), Times.Never);
        _mockEmbeddingCacheRepository.Verify(r => r.InsertAsync(It.IsAny<ProductEmbeddingCache>(), It.IsAny<bool>()), Times.Never);
    }
}
