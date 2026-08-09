namespace Nop.Plugin.Misc.UserNotifications.Services;

/// <summary>
/// Service for dispatching SMS notifications via FarazSMS
/// </summary>
public interface ISmsNotificationService
{
    /// <summary>
    /// Sends an SMS message to a recipient phone number
    /// </summary>
    Task<bool> SendSmsAsync(string phoneNumber, string messageText, string patternCode = null, IDictionary<string, string> patternValues = null);
}
