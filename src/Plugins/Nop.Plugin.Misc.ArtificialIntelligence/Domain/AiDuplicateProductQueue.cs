using Nop.Core;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Domain;

public class AiDuplicateProductQueue : BaseEntity
{
    public int ProductId { get; set; }
    public int VendorId { get; set; }
    public int DuplicateProductId { get; set; }
    public int StatusId { get; set; }
    public string Explanation { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }

    public DuplicateStatus Status
    {
        get => (DuplicateStatus)StatusId;
        set => StatusId = (int)value;
    }
}
