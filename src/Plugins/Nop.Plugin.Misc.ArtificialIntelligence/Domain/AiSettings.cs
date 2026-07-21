using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Domain;

public class AiSettings : ISettings
{
    public string ApiKey { get; set; }
    public string BaseUrl { get; set; } = "https://api.avalai.ir/v1";
    public bool SandboxMode { get; set; } = true;
    public AiProviderType ProviderType { get; set; } = AiProviderType.CloudAvalAi;
    
    // Local Infrastructure Settings
    public string LocalSttEndpoint { get; set; } = "http://localhost:8000/v1/audio/transcriptions";
    public string LocalEmbeddingEndpoint { get; set; } = "http://localhost:11434/api/embeddings";
    public string LocalVisionEndpoint { get; set; } = "http://localhost:11434/v1/chat/completions";
    public string LocalChatEndpoint { get; set; } = "http://localhost:11434/v1/chat/completions";
    public string LocalApiKey { get; set; } = "";
    public string LocalSttModel { get; set; } = "whisper-1";
    public string LocalEmbeddingModel { get; set; } = "bge-m3";
    public string LocalVisionModel { get; set; } = "qwen2-vl";
    public string LocalChatModel { get; set; } = "llama3";
    public bool EnableClientWebSpeechFallback { get; set; } = true;

    public string ChatbotModel { get; set; } = "gpt-4o-mini";
    public string VisionModel { get; set; } = "gpt-4o-mini";
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
    public decimal DuplicateSimilarityThreshold { get; set; } = 0.85m;
    public decimal CreditThreshold { get; set; } = 30000m;
}
