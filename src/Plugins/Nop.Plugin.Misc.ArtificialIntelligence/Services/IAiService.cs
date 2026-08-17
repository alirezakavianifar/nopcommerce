using Nop.Plugin.Misc.ArtificialIntelligence.Domain;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Services;

public interface IAiService
{
    Task<float[]> GetEmbeddingAsync(string text);
    Task<string> SpeechToTextAsync(byte[] audioData, string filename);
    Task<string> ChatResponseAsync(IList<object> history);
    Task<IList<int>> VisualSearchAsync(byte[] imageData);
    Task<AiDuplicateCheckResult> CheckDuplicateAsync(int productId);
    Task<IList<int>> TextSearchAsync(string query, int maxResults = 10);
}

public class AiDuplicateCheckResult
{
    public bool IsDuplicate { get; set; }
    public int DuplicateProductId { get; set; }
    public decimal Confidence { get; set; }
}
