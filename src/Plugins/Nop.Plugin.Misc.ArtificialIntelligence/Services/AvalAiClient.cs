using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Services;

public class AvalAiClient : IAvalAiClient
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
}
