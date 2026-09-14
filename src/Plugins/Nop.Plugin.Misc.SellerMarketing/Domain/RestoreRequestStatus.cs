namespace Nop.Plugin.Misc.SellerMarketing.Domain;

/// <summary>
/// Represents the status of a seller restore request
/// </summary>
public enum RestoreRequestStatus
{
    /// <summary>
    /// Pending admin approval
    /// </summary>
    PendingApproval = 10,

    /// <summary>
    /// Approved by admin and ready for execution
    /// </summary>
    Approved = 20,

    /// <summary>
    /// Rejected by admin
    /// </summary>
    Rejected = 30,

    /// <summary>
    /// Currently executing restore process
    /// </summary>
    Executing = 40,

    /// <summary>
    /// Successfully restored
    /// </summary>
    Completed = 50,

    /// <summary>
    /// Failed during restore execution
    /// </summary>
    Failed = 60
}
