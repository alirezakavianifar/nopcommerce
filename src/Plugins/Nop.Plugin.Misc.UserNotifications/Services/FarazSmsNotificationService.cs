using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Nop.Plugin.Misc.UserNotifications.Domain;

namespace Nop.Plugin.Misc.UserNotifications.Services;

/// <summary>
/// Service implementing SMS dispatching via FarazSMS / IPPanel API
/// </summary>
public class FarazSmsNotificationService : ISmsNotificationService
{
    private readonly FarazSmsSettings _farazSmsSettings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FarazSmsNotificationService> _logger;

    public FarazSmsNotificationService(
        FarazSmsSettings farazSmsSettings,
        IHttpClientFactory httpClientFactory,
        ILogger<FarazSmsNotificationService> logger)
    {
        _farazSmsSettings = farazSmsSettings;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string messageText, string patternCode = null, IDictionary<string, string> patternValues = null)
    {
        if (!_farazSmsSettings.Enabled)
        {
            _logger.LogInformation("FarazSMS is disabled. Skipping SMS dispatch to {Phone}", phoneNumber);
            return false;
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        try
        {
            var client = _httpClientFactory.CreateClient();

            // Sanitize phone number for Iranian SMS format (09xxxxxxxxx)
            var formattedPhone = phoneNumber.Trim().Replace("+98", "0").Replace(" ", "");

            if (!string.IsNullOrWhiteSpace(patternCode) || !string.IsNullOrWhiteSpace(_farazSmsSettings.DefaultPatternCode))
            {
                var codeToUse = !string.IsNullOrWhiteSpace(patternCode) ? patternCode : _farazSmsSettings.DefaultPatternCode;
                var payload = new
                {
                    op = "pattern",
                    user = _farazSmsSettings.ApiKey,
                    fromNum = _farazSmsSettings.SenderNumber,
                    toNum = formattedPhone,
                    patternCode = codeToUse,
                    inputData = patternValues ?? new Dictionary<string, string> { { "message", messageText } }
                };

                var response = await client.PostAsJsonAsync(_farazSmsSettings.ApiUrl, payload);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("FarazSMS Pattern Response: {Status} - {Body}", response.StatusCode, content);
                return response.IsSuccessStatusCode;
            }
            else
            {
                var payload = new
                {
                    op = "send",
                    uname = _farazSmsSettings.ApiKey,
                    pass = _farazSmsSettings.ApiKey,
                    from = _farazSmsSettings.SenderNumber,
                    to = new[] { formattedPhone },
                    message = messageText
                };

                var response = await client.PostAsJsonAsync(_farazSmsSettings.ApiUrl, payload);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("FarazSMS Direct Send Response: {Status} - {Body}", response.StatusCode, content);
                return response.IsSuccessStatusCode;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS via FarazSMS to {Phone}", phoneNumber);
            return false;
        }
    }
}
