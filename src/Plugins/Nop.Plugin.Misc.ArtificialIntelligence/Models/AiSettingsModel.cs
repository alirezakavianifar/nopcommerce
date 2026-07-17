namespace Nop.Plugin.Misc.ArtificialIntelligence.Models;

public class AiSettingsModel
{
    public string ApiKey { get; set; }
    public string BaseUrl { get; set; }
    public bool SandboxMode { get; set; }
    public string ChatbotModel { get; set; }
    public string VisionModel { get; set; }
    public string EmbeddingModel { get; set; }
    public decimal DuplicateSimilarityThreshold { get; set; }
    public decimal CreditThreshold { get; set; }
    public decimal? CurrentCredit { get; set; }
}
