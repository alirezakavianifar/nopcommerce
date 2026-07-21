using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Nop.Plugin.Misc.ArtificialIntelligence.Domain;
using Nop.Plugin.Misc.ArtificialIntelligence.Services;
using Nop.Services.Logging;

namespace Nop.Tests.Nop.Web.Tests.Events;

[TestFixture]
public class LocalAiInfrastructureTests
{
    private Mock<ILogger> _mockLogger;
    private Mock<IHttpClientFactory> _mockHttpClientFactory;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
    }

    [Test]
    public void AiProviderFactory_ResolvesLocalAiClient_WhenProviderTypeIsLocalInfrastructure()
    {
        var avalClient = new AvalAiClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var localClient = new LocalAiClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var factory = new AiProviderFactory(avalClient, localClient);

        var settings = new AiSettings
        {
            SandboxMode = false,
            ProviderType = AiProviderType.LocalInfrastructure
        };

        var resolvedClient = factory.GetClient(settings);
        Assert.That(resolvedClient, Is.SameAs(localClient));
    }

    [Test]
    public void AiProviderFactory_ResolvesAvalAiClient_WhenProviderTypeIsCloudAvalAi()
    {
        var avalClient = new AvalAiClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var localClient = new LocalAiClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var factory = new AiProviderFactory(avalClient, localClient);

        var settings = new AiSettings
        {
            SandboxMode = false,
            ProviderType = AiProviderType.CloudAvalAi
        };

        var resolvedClient = factory.GetClient(settings);
        Assert.That(resolvedClient, Is.SameAs(avalClient));
    }

    [Test]
    public async Task LocalAiClient_GetEmbeddingAsync_ParsesOllamaFormatCorrectly()
    {
        var mockHandler = new MockHttpMessageHandler(@"{""embedding"": [0.1, 0.2, 0.3, 0.4]}");
        var client = new HttpClient(mockHandler);
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

        var localAiClient = new LocalAiClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var settings = new AiSettings
        {
            LocalEmbeddingEndpoint = "http://localhost:11434/api/embeddings",
            LocalEmbeddingModel = "bge-m3"
        };

        var result = await localAiClient.GetEmbeddingAsync("test search query", settings);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.EqualTo(4));
        Assert.That(result[0], Is.EqualTo(0.1f).Within(0.001f));
    }

    [Test]
    public async Task LocalAiClient_SpeechToTextAsync_ParsesWhisperTranscriptionCorrectly()
    {
        var mockHandler = new MockHttpMessageHandler(@"{""text"": ""کفش ورزشی""}");
        var client = new HttpClient(mockHandler);
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

        var localAiClient = new LocalAiClient(_mockHttpClientFactory.Object, _mockLogger.Object);
        var settings = new AiSettings
        {
            LocalSttEndpoint = "http://localhost:8000/v1/audio/transcriptions",
            LocalSttModel = "whisper-1"
        };

        var audioBytes = new byte[] { 1, 2, 3, 4, 5 };
        var transcription = await localAiClient.SpeechToTextAsync(audioBytes, "voice.webm", settings);

        Assert.That(transcription, Is.EqualTo("کفش ورزشی"));
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseContent;

        public MockHttpMessageHandler(string responseContent)
        {
            _responseContent = responseContent;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseContent, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
