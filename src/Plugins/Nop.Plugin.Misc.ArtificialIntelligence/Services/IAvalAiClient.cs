namespace Nop.Plugin.Misc.ArtificialIntelligence.Services;

public interface IAvalAiClient
{
    Task<float[]> GetEmbeddingAsync(string text, string apiKey, string model, string baseUrl);
    Task<string> GetChatResponseAsync(IList<object> messages, string apiKey, string model, string baseUrl);
    Task<string> SpeechToTextAsync(byte[] audioData, string filename, string apiKey, string baseUrl);
    Task<string> AnalyzeImageAsync(byte[] imageData, string prompt, string apiKey, string model, string baseUrl);
    Task<AvalAiCreditResponse> GetCreditAsync(string apiKey, string baseUrl);
}
