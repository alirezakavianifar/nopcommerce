using Nop.Core;

namespace Nop.Plugin.Misc.UserNotifications.Domain;

/// <summary>
/// Represents a record of a customer viewing a product
/// </summary>
public partial class ProductViewLog : BaseEntity
{
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public DateTime ViewedOnUtc { get; set; }
}
