using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Domain;

public class AiSettings : ISettings
{
    public string ApiKey { get; set; }
    public string BaseUrl { get; set; } = "https://api.avalai.ir/v1";
    public bool SandboxMode { get; set; } = true;
    public string ChatbotModel { get; set; } = "gpt-5.5";
    public string VisionModel { get; set; } = "gpt-5.5";
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
    public decimal DuplicateSimilarityThreshold { get; set; } = 0.85m;
    public decimal CreditThreshold { get; set; } = 30000m;
}
