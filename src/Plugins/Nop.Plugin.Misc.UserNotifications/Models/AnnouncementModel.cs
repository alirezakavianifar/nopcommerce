using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.UserNotifications.Models;

public record AnnouncementModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Misc.UserNotifications.Fields.Title")]
    public string Title { get; set; }

    [NopResourceDisplayName("Plugins.Misc.UserNotifications.Fields.Body")]
    [UIHint("RichEditor")]
    public string Body { get; set; }

    [NopResourceDisplayName("Plugins.Misc.UserNotifications.Fields.StartDateUtc")]
    public DateTime? StartDateUtc { get; set; }

    [NopResourceDisplayName("Plugins.Misc.UserNotifications.Fields.EndDateUtc")]
    public DateTime? EndDateUtc { get; set; }

    [NopResourceDisplayName("Plugins.Misc.UserNotifications.Fields.IsPublished")]
    public bool IsPublished { get; set; }

    public DateTime CreatedOnUtc { get; set; }
}
