using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Nop.Plugin.Misc.ArtificialIntelligence.Domain;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Services;

public class LocalAiClient : IAiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    public LocalAiClient(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private HttpClient CreateClient(string apiKey)
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(60);
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }
        return client;
    }

    public async Task<float[]> GetEmbeddingAsync(string text, AiSettings settings)
    {
        try
        {
            var endpoint = string.IsNullOrWhiteSpace(settings.LocalEmbeddingEndpoint)
                ? "http://localhost:11434/api/embeddings"
                : settings.LocalEmbeddingEndpoint;

            var client = CreateClient(settings.LocalApiKey);
            var model = string.IsNullOrWhiteSpace(settings.LocalEmbeddingModel) ? "bge-m3" : settings.LocalEmbeddingModel;

            // Try standard OpenAI schema first or Ollama schema depending on endpoint path
            object requestBody;
            if (endpoint.Contains("/api/embeddings"))
            {
                // Ollama format
                requestBody = new { model = model, prompt = text };
            }
            else
            {
                // Standard OpenAI embedding format
                requestBody = new { model = model, input = text };
            }

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                await _logger.WarningAsync($"Local AI Embedding error from {endpoint}: {response.StatusCode} - {err}");
                return Array.Empty<float>();
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;

            // 1. Check standard OpenAI format: data[0].embedding
            if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Array && dataProp.GetArrayLength() > 0)
            {
                if (dataProp[0].TryGetProperty("embedding", out var embeddingProp) && embeddingProp.ValueKind == JsonValueKind.Array)
                {
                    return ParseFloatArray(embeddingProp);
                }
            }

            // 2. Check Ollama format: embedding: [...]
            if (root.TryGetProperty("embedding", out var directEmbedding) && directEmbedding.ValueKind == JsonValueKind.Array)
            {
                return ParseFloatArray(directEmbedding);
            }

            return Array.Empty<float>();
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Local AI Exception in GetEmbeddingAsync targeting {settings.LocalEmbeddingEndpoint}", ex);
            return Array.Empty<float>();
        }
    }

    public async Task<string> SpeechToTextAsync(byte[] audioData, string filename, AiSettings settings)
    {
        try
        {
            var endpoint = string.IsNullOrWhiteSpace(settings.LocalSttEndpoint)
                ? "http://localhost:8000/v1/audio/transcriptions"
                : settings.LocalSttEndpoint;

            var client = CreateClient(settings.LocalApiKey);

            using var form = new MultipartFormDataContent();
            var audioContent = new ByteArrayContent(audioData);
            audioContent.Headers.ContentType = MediaTypeHeaderValue.Parse(filename.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ? "audio/wav" : "audio/webm");
            
            form.Add(audioContent, "file", filename);
            form.Add(new StringContent(string.IsNullOrWhiteSpace(settings.LocalSttModel) ? "whisper-1" : settings.LocalSttModel), "model");

            var response = await client.PostAsync(endpoint, form);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                await _logger.WarningAsync($"Local STT error from {endpoint}: {response.StatusCode} - {err}");
                return string.Empty;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            
            if (doc.RootElement.TryGetProperty("text", out var textProp))
            {
                return textProp.GetString() ?? string.Empty;
            }

            if (doc.RootElement.TryGetProperty("transcription", out var transProp))
            {
                return transProp.GetString() ?? string.Empty;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Local AI Exception in SpeechToTextAsync targeting {settings.LocalSttEndpoint}", ex);
            return string.Empty;
        }
    }

    public async Task<string> AnalyzeImageAsync(byte[] imageData, string prompt, AiSettings settings)
    {
        try
        {
            var endpoint = string.IsNullOrWhiteSpace(settings.LocalVisionEndpoint)
                ? "http://localhost:11434/v1/chat/completions"
                : settings.LocalVisionEndpoint;

            var client = CreateClient(settings.LocalApiKey);
            var base64Image = Convert.ToBase64String(imageData);
            var imageUrl = $"data:image/jpeg;base64,{base64Image}";

            var model = string.IsNullOrWhiteSpace(settings.LocalVisionModel) ? "qwen2-vl" : settings.LocalVisionModel;

            var messages = new List<object>
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = prompt },
                        new { type = "image_url", image_url = new { url = imageUrl } }
                    }
                }
            };

            var requestBody = new { model = model, messages = messages };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                await _logger.WarningAsync($"Local Vision error from {endpoint}: {response.StatusCode} - {err}");
                return string.Empty;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                return choices[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Local AI Exception in AnalyzeImageAsync targeting {settings.LocalVisionEndpoint}", ex);
            return string.Empty;
        }
    }

    public async Task<string> GetChatResponseAsync(IList<object> messages, AiSettings settings)
    {
        try
        {
            var endpoint = string.IsNullOrWhiteSpace(settings.LocalChatEndpoint)
                ? "http://localhost:11434/v1/chat/completions"
                : settings.LocalChatEndpoint;

            var client = CreateClient(settings.LocalApiKey);
            var model = string.IsNullOrWhiteSpace(settings.LocalChatModel) ? "llama3" : settings.LocalChatModel;

            var requestBody = new { model = model, messages = messages };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                await _logger.WarningAsync($"Local Chat error from {endpoint}: {response.StatusCode} - {err}");
                return string.Empty;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                return choices[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Local AI Exception in GetChatResponseAsync targeting {settings.LocalChatEndpoint}", ex);
            return string.Empty;
        }
    }

    private static float[] ParseFloatArray(JsonElement element)
    {
        var length = element.GetArrayLength();
        var result = new float[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = element[i].GetSingle();
        }
        return result;
    }
}
