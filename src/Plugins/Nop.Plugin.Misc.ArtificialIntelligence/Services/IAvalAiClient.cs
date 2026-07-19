using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Services;

public class AvalAiModelInfo
{
    public string Id { get; set; }
    public string OwnedBy { get; set; }
    public decimal InputPrice { get; set; }
    public decimal OutputPrice { get; set; }
    public bool SupportsVision { get; set; }
    public string Mode { get; set; }
}

public interface IAvalAiClient
{
    Task<float[]> GetEmbeddingAsync(string text, string apiKey, string model, string baseUrl);
    Task<string> GetChatResponseAsync(IList<object> messages, string apiKey, string model, string baseUrl);
    Task<string> SpeechToTextAsync(byte[] audioData, string filename, string apiKey, string baseUrl);
    Task<string> AnalyzeImageAsync(byte[] imageData, string prompt, string apiKey, string model, string baseUrl);
    Task<AvalAiCreditResponse> GetCreditAsync(string apiKey, string baseUrl);
    Task<List<AvalAiModelInfo>> GetModelsAsync(string apiKey, string baseUrl);
}
