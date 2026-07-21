using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Nop.Services.Logging;

using Nop.Plugin.Misc.ArtificialIntelligence.Domain;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Services;

public class AvalAiClient : IAvalAiClient, IAiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    public AvalAiClient(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private HttpClient CreateClient(string apiKey, string baseUrl)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return client;
    }

    public async Task<float[]> GetEmbeddingAsync(string text, string apiKey, string model, string baseUrl)
    {
        try
        {
            var client = CreateClient(apiKey, baseUrl);
            var requestBody = new
            {
                input = text,
                model = model
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("embeddings", content);

            if (!response.IsSuccessStatusCode)
            {
                var errContent = await response.Content.ReadAsStringAsync();
                await _logger.ErrorAsync($"AvalAI embeddings error: {response.StatusCode} - {errContent}");
                return Array.Empty<float>();
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;
            if (root.TryGetProperty("data", out var dataProp) && dataProp.GetArrayLength() > 0)
            {
                var embeddingProp = dataProp[0].GetProperty("embedding");
                var length = embeddingProp.GetArrayLength();
                var result = new float[length];
                for (int i = 0; i < length; i++)
                {
                    result[i] = embeddingProp[i].GetSingle();
                }
                return result;
            }

            return Array.Empty<float>();
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("AvalAI Exception in GetEmbeddingAsync", ex);
            return Array.Empty<float>();
        }
    }

    public async Task<string> GetChatResponseAsync(IList<object> messages, string apiKey, string model, string baseUrl)
    {
        try
        {
            var client = CreateClient(apiKey, baseUrl);
            var requestBody = new
            {
                model = model,
                messages = messages
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errContent = await response.Content.ReadAsStringAsync();
                await _logger.ErrorAsync($"AvalAI completions error: {response.StatusCode} - {errContent}");
                return string.Empty;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            var choices = doc.RootElement.GetProperty("choices");
            if (choices.GetArrayLength() > 0)
            {
                return choices[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("AvalAI Exception in GetChatResponseAsync", ex);
            return string.Empty;
        }
    }

    public async Task<string> SpeechToTextAsync(byte[] audioData, string filename, string apiKey, string baseUrl)
    {
        try
        {
            var client = CreateClient(apiKey, baseUrl);
            
            using var form = new MultipartFormDataContent();
            var audioContent = new ByteArrayContent(audioData);
            audioContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav"); // default or WebM
            form.Add(audioContent, "file", filename);
            form.Add(new StringContent("whisper-1"), "model"); // default STT model in OpenAI spec

            var response = await client.PostAsync("audio/transcriptions", form);

            if (!response.IsSuccessStatusCode)
            {
                var errContent = await response.Content.ReadAsStringAsync();
                await _logger.ErrorAsync($"AvalAI STT error: {response.StatusCode} - {errContent}");
                return string.Empty;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            if (doc.RootElement.TryGetProperty("text", out var textProp))
            {
                return textProp.GetString() ?? string.Empty;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("AvalAI Exception in SpeechToTextAsync", ex);
            return string.Empty;
        }
    }

    public async Task<string> AnalyzeImageAsync(byte[] imageData, string prompt, string apiKey, string model, string baseUrl)
    {
        try
        {
            var client = CreateClient(apiKey, baseUrl);
            var base64Image = Convert.ToBase64String(imageData);
            var imageUrl = $"data:image/jpeg;base64,{base64Image}";

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

            var requestBody = new
            {
                model = model,
                messages = messages
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errContent = await response.Content.ReadAsStringAsync();
                await _logger.ErrorAsync($"AvalAI Vision error: {response.StatusCode} - {errContent}");
                return string.Empty;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            var choices = doc.RootElement.GetProperty("choices");
            if (choices.GetArrayLength() > 0)
            {
                return choices[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("AvalAI Exception in AnalyzeImageAsync", ex);
            return string.Empty;
        }
    }

    public async Task<AvalAiCreditResponse> GetCreditAsync(string apiKey, string baseUrl)
    {
        try
        {
            var client = CreateClient(apiKey, baseUrl);
            
            // Resolve the absolute URL relative to base URL (authority-level rewrite)
            string creditUrl;
            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            {
                creditUrl = $"{baseUri.Scheme}://{baseUri.Authority}/user/v1/credit";
            }
            else
            {
                creditUrl = "https://api.avalai.ir/user/v1/credit";
            }

            var response = await client.GetAsync(creditUrl);

            if (!response.IsSuccessStatusCode)
            {
                var errContent = await response.Content.ReadAsStringAsync();
                await _logger.ErrorAsync($"AvalAI credit check error: {response.StatusCode} - {errContent}");
                return null;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<AvalAiCreditResponse>(jsonString, options);
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("AvalAI Exception in GetCreditAsync", ex);
            return null;
        }
    }

    private static readonly List<AvalAiModelInfo> DefaultModels = new()
    {
        // Chat & Vision Models
        new AvalAiModelInfo { Id = "gpt-5.5", OwnedBy = "openai", InputPrice = 5.0m, OutputPrice = 30.0m, SupportsVision = true, Mode = "chat" },
        new AvalAiModelInfo { Id = "gpt-4o-mini", OwnedBy = "openai", InputPrice = 0.15m, OutputPrice = 0.60m, SupportsVision = true, Mode = "chat" },
        new AvalAiModelInfo { Id = "gemini-2.5-flash", OwnedBy = "google", InputPrice = 0.30m, OutputPrice = 2.50m, SupportsVision = true, Mode = "chat" },
        new AvalAiModelInfo { Id = "deepseek-chat", OwnedBy = "deepseek", InputPrice = 0.14m, OutputPrice = 0.28m, SupportsVision = false, Mode = "chat" },
        new AvalAiModelInfo { Id = "claude-sonnet-4-6", OwnedBy = "anthropic", InputPrice = 3.0m, OutputPrice = 15.0m, SupportsVision = false, Mode = "chat" },
        new AvalAiModelInfo { Id = "gemini-2.5-flash-lite", OwnedBy = "google", InputPrice = 0.10m, OutputPrice = 0.40m, SupportsVision = true, Mode = "chat" },
        new AvalAiModelInfo { Id = "qwen3-vl-flash", OwnedBy = "alibaba", InputPrice = 0.05m, OutputPrice = 0.40m, SupportsVision = true, Mode = "chat" },
        new AvalAiModelInfo { Id = "gpt-5-nano", OwnedBy = "openai", InputPrice = 0.05m, OutputPrice = 0.40m, SupportsVision = true, Mode = "chat" },

        // Embedding Models
        new AvalAiModelInfo { Id = "text-embedding-3-small", OwnedBy = "openai", InputPrice = 0.02m, OutputPrice = 0.02m, SupportsVision = false, Mode = "embedding" },
        new AvalAiModelInfo { Id = "text-embedding-3-large", OwnedBy = "openai", InputPrice = 0.13m, OutputPrice = 0.13m, SupportsVision = false, Mode = "embedding" },
        new AvalAiModelInfo { Id = "nvidia_nim.bge-m3", OwnedBy = "baai", InputPrice = 0.002m, OutputPrice = 0.002m, SupportsVision = false, Mode = "embedding" }
    };

    public async Task<List<AvalAiModelInfo>> GetModelsAsync(string apiKey, string baseUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return DefaultModels;
            }

            var client = CreateClient(apiKey, baseUrl);
            var response = await client.GetAsync("models");
            if (!response.IsSuccessStatusCode)
            {
                var errContent = await response.Content.ReadAsStringAsync();
                await _logger.WarningAsync($"AvalAI GetModelsAsync error: {response.StatusCode} - {errContent}. Falling back to default list.");
                return DefaultModels;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            var result = new List<AvalAiModelInfo>();
            if (doc.RootElement.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in dataProp.EnumerateArray())
                {
                    var id = item.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                    if (string.IsNullOrEmpty(id))
                        continue;

                    var ownedBy = (item.TryGetProperty("owned_by", out var ownedProp) ? ownedProp.GetString() : "unknown") ?? "unknown";
                    var mode = (item.TryGetProperty("mode", out var modeProp) ? modeProp.GetString() : "") ?? "";
                    var supportsVision = item.TryGetProperty("supports_vision", out var visionProp) && visionProp.GetBoolean();

                    decimal inputPrice = 0;
                    decimal outputPrice = 0;
                    if (item.TryGetProperty("pricing", out var pricingProp))
                    {
                        if (pricingProp.TryGetProperty("input", out var inputProp))
                        {
                            inputProp.TryGetDecimal(out inputPrice);
                        }
                        if (pricingProp.TryGetProperty("output", out var outputProp))
                        {
                            outputProp.TryGetDecimal(out outputPrice);
                        }
                    }

                    result.Add(new AvalAiModelInfo
                    {
                        Id = id,
                        OwnedBy = ownedBy,
                        InputPrice = inputPrice,
                        OutputPrice = outputPrice,
                        SupportsVision = supportsVision,
                        Mode = mode
                    });
                }
            }

            if (result.Count == 0)
            {
                return DefaultModels;
            }

            return result;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("AvalAI Exception in GetModelsAsync. Falling back to default list.", ex);
            return DefaultModels;
        }
    }

    #region IAiClient Implementation

    public Task<float[]> GetEmbeddingAsync(string text, AiSettings settings)
    {
        return GetEmbeddingAsync(text, settings.ApiKey, settings.EmbeddingModel, settings.BaseUrl);
    }

    public Task<string> SpeechToTextAsync(byte[] audioData, string filename, AiSettings settings)
    {
        return SpeechToTextAsync(audioData, filename, settings.ApiKey, settings.BaseUrl);
    }

    public Task<string> AnalyzeImageAsync(byte[] imageData, string prompt, AiSettings settings)
    {
        return AnalyzeImageAsync(imageData, prompt, settings.ApiKey, settings.VisionModel, settings.BaseUrl);
    }

    public Task<string> GetChatResponseAsync(IList<object> messages, AiSettings settings)
    {
        return GetChatResponseAsync(messages, settings.ApiKey, settings.ChatbotModel, settings.BaseUrl);
    }

    #endregion
}
