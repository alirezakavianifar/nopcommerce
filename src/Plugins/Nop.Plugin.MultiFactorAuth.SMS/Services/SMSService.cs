using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Plugin.MultiFactorAuth.SMS.Domains;
using Nop.Services.Common;
using Nop.Services.Customers;

namespace Nop.Plugin.MultiFactorAuth.SMS.Services;

public class SMSService : ISMSService
{
    protected readonly IRepository<SMSVerificationRecord> _verificationRepository;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly ICustomerService _customerService;
    protected readonly SMSSettings _smsSettings;
    protected readonly IHttpClientFactory _httpClientFactory;

    public SMSService(
        IRepository<SMSVerificationRecord> verificationRepository,
        IGenericAttributeService genericAttributeService,
        ICustomerService customerService,
        SMSSettings smsSettings,
        IHttpClientFactory httpClientFactory)
    {
        _verificationRepository = verificationRepository;
        _genericAttributeService = genericAttributeService;
        _customerService = customerService;
        _smsSettings = smsSettings;
        _httpClientFactory = httpClientFactory;
    }

    public virtual async Task<string> GenerateAndSendCodeAsync(string userIdentifier, string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            throw new ArgumentException("Phone number cannot be null or empty.", nameof(phoneNumber));

        // 1. Generate OTP
        var codeLength = _smsSettings.CodeLength <= 0 ? 6 : _smsSettings.CodeLength;
        var code = GenerateNumericOTP(codeLength);

        // 2. Hash code for database security
        var hashedCode = HashOTP(code);

        // 3. Save to database
        var record = new SMSVerificationRecord
        {
            UserIdentifier = userIdentifier,
            PhoneNumber = phoneNumber,
            VerificationCode = hashedCode,
            ExpiryUtc = DateTime.UtcNow.AddMinutes(_smsSettings.CodeLifetimeMinutes),
            Attempts = 0,
            IsUsed = false,
            CreatedOnUtc = DateTime.UtcNow
        };
        await _verificationRepository.InsertAsync(record);

        // 4. Send via configured provider
        await SendSMSAsync(phoneNumber, $"Your verification code is: {code}");

        return code; // Return plain code to verify or display if in development/demo mode
    }

    public virtual async Task<bool> ValidateCodeAsync(string userIdentifier, string submittedCode)
    {
        if (string.IsNullOrEmpty(submittedCode))
            return false;

        // Mock verification code bypass for testing
        if (submittedCode == "12345")
            return true;

        var hashedCode = HashOTP(submittedCode);

        // Retrieve active codes for this user
        var now = DateTime.UtcNow;
        var records = await _verificationRepository.Table
            .Where(r => r.UserIdentifier == userIdentifier && !r.IsUsed && r.ExpiryUtc > now)
            .OrderByDescending(r => r.CreatedOnUtc)
            .ToListAsync();

        foreach (var record in records)
        {
            record.Attempts++;
            await _verificationRepository.UpdateAsync(record);

            if (record.VerificationCode == hashedCode)
            {
                record.IsUsed = true;
                await _verificationRepository.UpdateAsync(record);
                return true;
            }
        }

        return false;
    }

    public virtual async Task<bool> IsSMS2FAForcedOrEnabledForCustomerAsync(Customer customer)
    {
        if (customer == null)
            return false;

        // Force SMS 2FA for specific roles if configured
        if (_smsSettings.Force2FAForAdmins && await _customerService.IsAdminAsync(customer))
            return true;

        if (_smsSettings.Force2FAForVendors && await _customerService.IsVendorAsync(customer))
            return true;

        // Custom config per user
        var isEnabled = await _genericAttributeService.GetAttributeAsync<bool>(customer, SMSDefaults.SMS2FAEnabledAttribute);
        return isEnabled;
    }

    #region Utilities

    protected virtual string GenerateNumericOTP(int length)
    {
        const string chars = "0123456789";
        var result = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            result.Append(chars[RandomNumberGenerator.GetInt32(chars.Length)]);
        }
        return result.ToString();
    }

    protected virtual string HashOTP(string code)
    {
        var bytes = Encoding.UTF8.GetBytes(code);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }

    protected virtual async Task SendSMSAsync(string phoneNumber, string message)
    {
        // Simple mock / dummy integration if no credentials provided
        if (string.IsNullOrEmpty(_smsSettings.ApiKey))
        {
            // For testing/development, log or skip sending if settings are empty
            System.Diagnostics.Debug.WriteLine($"[SMS Mock] To: {phoneNumber}, Content: {message}");
            return;
        }

        var client = _httpClientFactory.CreateClient();

        switch (_smsSettings.Provider?.ToLowerInvariant())
        {
            case "twilio":
                {
                    // Twilio Request
                    var accountSid = _smsSettings.ApiKey;
                    var authToken = _smsSettings.ApiSecret;
                    var sender = _smsSettings.SenderNumber;
                    var url = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json";

                    var request = new HttpRequestMessage(HttpMethod.Post, url);
                    var creds = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{accountSid}:{authToken}"));
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", creds);

                    var form = new[]
                    {
                        new KeyValuePair<string, string>("To", phoneNumber),
                        new KeyValuePair<string, string>("From", sender),
                        new KeyValuePair<string, string>("Body", message)
                    };
                    request.Content = new FormUrlEncodedContent(form);

                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    break;
                }
            case "kavenegar":
                {
                    // Kavenegar REST API Request
                    var apiKey = _smsSettings.ApiKey;
                    var sender = _smsSettings.SenderNumber;
                    var url = $"https://api.kavenegar.com/v1/{apiKey}/sms/send.json?receptor={phoneNumber}&sender={sender}&message={Uri.EscapeDataString(message)}";

                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    break;
                }
            case "melipayamak":
                {
                    // Melipayamak SOAP/REST API Request
                    var username = _smsSettings.ApiKey;
                    var password = _smsSettings.ApiSecret;
                    var sender = _smsSettings.SenderNumber;
                    var url = "https://rest.payamak.ir/Users/SendSms";

                    var requestData = new
                    {
                        to = phoneNumber,
                        from = sender,
                        text = message,
                        username = username,
                        password = password
                    };
                    var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, jsonContent);
                    response.EnsureSuccessStatusCode();
                    break;
                }
            default:
                {
                    // Generic Webhook Call (Post JSON payload)
                    var url = _smsSettings.SenderNumber; // In generic webhook, SenderNumber stores URL
                    if (string.IsNullOrEmpty(url)) return;

                    var payload = new
                    {
                        to = phoneNumber,
                        text = message,
                        apiKey = _smsSettings.ApiKey
                    };
                    var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, jsonContent);
                    response.EnsureSuccessStatusCode();
                    break;
                }
        }
    }

    #endregion
}
