namespace Nop.Plugin.Misc.UserNotifications.Domain;

/// <summary>
/// Represents workflow trigger types
/// </summary>
public enum NotificationTriggerType
{
    /// <summary>
    /// Triggered when a customer completes registration
    /// </summary>
    CustomerRegistered = 1,

    /// <summary>
    /// Triggered when an order is placed
    /// </summary>
    OrderPlaced = 2,

    /// <summary>
    /// Triggered when a product page is viewed
    /// </summary>
    ProductViewed = 3,

    /// <summary>
    /// Triggered when a product is added to wishlist / bookmarked
    /// </summary>
    WishlistAdded = 4
}
