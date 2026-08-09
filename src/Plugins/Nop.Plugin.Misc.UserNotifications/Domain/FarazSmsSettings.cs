using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.UserNotifications.Domain;

/// <summary>
/// Represents FarazSMS configuration settings
/// </summary>
public class FarazSmsSettings : ISettings
{
    public bool Enabled { get; set; }
    public string ApiUrl { get; set; } = "https://ippanel.com/api/select";
    public string ApiKey { get; set; }
    public string SenderNumber { get; set; } = "+983000505";
    public string DefaultPatternCode { get; set; }
}
