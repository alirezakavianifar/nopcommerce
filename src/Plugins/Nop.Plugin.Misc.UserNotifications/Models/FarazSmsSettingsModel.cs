using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.UserNotifications.Models;

public record FarazSmsSettingsModel : BaseNopModel
{
    public bool Enabled { get; set; }
    public string ApiUrl { get; set; }
    public string ApiKey { get; set; }
    public string SenderNumber { get; set; }
    public string DefaultPatternCode { get; set; }
}
