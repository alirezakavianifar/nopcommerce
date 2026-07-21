using System.Collections.Generic;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Models;

public class AiSettingsModel
{
    public string ApiKey { get; set; }
    public string BaseUrl { get; set; }
    public bool SandboxMode { get; set; }
    public int ProviderTypeId { get; set; }

    // Local Infrastructure Settings
    public string LocalSttEndpoint { get; set; }
    public string LocalEmbeddingEndpoint { get; set; }
    public string LocalVisionEndpoint { get; set; }
    public string LocalChatEndpoint { get; set; }
    public string LocalApiKey { get; set; }
    public string LocalSttModel { get; set; }
    public string LocalEmbeddingModel { get; set; }
    public string LocalVisionModel { get; set; }
    public string LocalChatModel { get; set; }
    public bool EnableClientWebSpeechFallback { get; set; }

    public string ChatbotModel { get; set; }
    public string VisionModel { get; set; }
    public string EmbeddingModel { get; set; }
    public decimal DuplicateSimilarityThreshold { get; set; }
    public decimal CreditThreshold { get; set; }
    public decimal? CurrentCredit { get; set; }

    public IList<AvalAiModelDto> AvailableChatbotModels { get; set; } = new List<AvalAiModelDto>();
    public IList<AvalAiModelDto> AvailableVisionModels { get; set; } = new List<AvalAiModelDto>();
    public IList<AvalAiModelDto> AvailableEmbeddingModels { get; set; } = new List<AvalAiModelDto>();
}

public class AvalAiModelDto
{
    public string Value { get; set; }
    public string Text { get; set; }
    public string InputPrice { get; set; }
    public string OutputPrice { get; set; }
    public string Provider { get; set; }
    public string SupportsVision { get; set; }
}
