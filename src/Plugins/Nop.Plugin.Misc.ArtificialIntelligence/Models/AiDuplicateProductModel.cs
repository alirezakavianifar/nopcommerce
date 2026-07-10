using Nop.Plugin.Misc.ArtificialIntelligence.Domain;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Models;

public class AiDuplicateProductModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductSku { get; set; }
    public int DuplicateProductId { get; set; }
    public string DuplicateProductName { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; }
    public string Status { get; set; }
    public int StatusId { get; set; }
    public string Explanation { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
