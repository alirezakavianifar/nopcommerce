using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.UserNotifications.Domain;

namespace Nop.Plugin.Misc.UserNotifications.Services;

/// <summary>
/// User notification service
/// </summary>
public partial class UserNotificationService : IUserNotificationService
{
    protected readonly IRepository<NotificationAnnouncement> _announcementRepository;

    public UserNotificationService(IRepository<NotificationAnnouncement> announcementRepository)
    {
        _announcementRepository = announcementRepository;
    }

    /// <summary>
    /// Gets active announcements
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of active announcements</returns>
    public virtual async Task<IList<NotificationAnnouncement>> GetActiveAnnouncementsAsync()
    {
        var nowUtc = DateTime.UtcNow;

        return await _announcementRepository.GetAllAsync(query =>
        {
            query = query.Where(a => a.IsPublished);
            query = query.Where(a => !a.StartDateUtc.HasValue || a.StartDateUtc <= nowUtc);
            query = query.Where(a => !a.EndDateUtc.HasValue || a.EndDateUtc >= nowUtc);
            query = query.OrderByDescending(a => a.CreatedOnUtc);

            return query;
        });
    }

    /// <summary>
    /// Gets an announcement by identifier
    /// </summary>
    /// <param name="announcementId">Announcement identifier</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the announcement</returns>
    public virtual async Task<NotificationAnnouncement> GetAnnouncementByIdAsync(int announcementId)
    {
        return await _announcementRepository.GetByIdAsync(announcementId);
    }

    /// <summary>
    /// Inserts an announcement
    /// </summary>
    /// <param name="announcement">Announcement</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertAnnouncementAsync(NotificationAnnouncement announcement)
    {
        await _announcementRepository.InsertAsync(announcement);
    }

    /// <summary>
    /// Updates an announcement
    /// </summary>
    /// <param name="announcement">Announcement</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task UpdateAnnouncementAsync(NotificationAnnouncement announcement)
    {
        await _announcementRepository.UpdateAsync(announcement);
    }

    /// <summary>
    /// Deletes an announcement
    /// </summary>
    /// <param name="announcement">Announcement</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task DeleteAnnouncementAsync(NotificationAnnouncement announcement)
    {
        await _announcementRepository.DeleteAsync(announcement);
    }

    /// <summary>
    /// Gets all announcements (paged)
    /// </summary>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="showHidden">A value indicating whether to show hidden records</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the paged list of announcements</returns>
    public virtual async Task<IPagedList<NotificationAnnouncement>> GetAllAnnouncementsAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
    {
        return await _announcementRepository.GetAllPagedAsync(query =>
        {
            if (!showHidden)
                query = query.Where(a => a.IsPublished);

            query = query.OrderByDescending(a => a.CreatedOnUtc);

            return query;
        }, pageIndex, pageSize);
    }
}
