using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Misc.ArtificialIntelligence.Domain;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Services;

public interface IAiClient
{
    Task<float[]> GetEmbeddingAsync(string text, AiSettings settings);
    Task<string> SpeechToTextAsync(byte[] audioData, string filename, AiSettings settings);
    Task<string> AnalyzeImageAsync(byte[] imageData, string prompt, AiSettings settings);
    Task<string> GetChatResponseAsync(IList<object> messages, AiSettings settings);
}
