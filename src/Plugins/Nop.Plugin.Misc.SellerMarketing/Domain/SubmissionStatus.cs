namespace Nop.Plugin.Misc.SellerMarketing.Domain;

/// <summary>
/// Represents seller catalog submission status
/// </summary>
public enum SubmissionStatus
{
    /// <summary>
    /// Pending approval
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Approved (and published)
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Rejected
    /// </summary>
    Rejected = 2
}
