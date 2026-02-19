using Nop.Core;
using Nop.Plugin.Misc.UserNotifications.Domain;

namespace Nop.Plugin.Misc.UserNotifications.Services;

/// <summary>
/// User notification service interface
/// </summary>
public partial interface IUserNotificationService
{
    /// <summary>
    /// Gets active announcements
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of active announcements</returns>
    Task<IList<NotificationAnnouncement>> GetActiveAnnouncementsAsync();

    /// <summary>
    /// Gets an announcement by identifier
    /// </summary>
    /// <param name="announcementId">Announcement identifier</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the announcement</returns>
    Task<NotificationAnnouncement> GetAnnouncementByIdAsync(int announcementId);

    /// <summary>
    /// Inserts an announcement
    /// </summary>
    /// <param name="announcement">Announcement</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertAnnouncementAsync(NotificationAnnouncement announcement);

    /// <summary>
    /// Updates an announcement
    /// </summary>
    /// <param name="announcement">Announcement</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateAnnouncementAsync(NotificationAnnouncement announcement);

    /// <summary>
    /// Deletes an announcement
    /// </summary>
    /// <param name="announcement">Announcement</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteAnnouncementAsync(NotificationAnnouncement announcement);

    /// <summary>
    /// Gets all announcements (paged)
    /// </summary>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="showHidden">A value indicating whether to show hidden records</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the paged list of announcements</returns>
    Task<IPagedList<NotificationAnnouncement>> GetAllAnnouncementsAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
}
