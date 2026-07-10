using Nop.Core;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Domain;

public class ProductEmbeddingCache : BaseEntity
{
    public int ProductId { get; set; }
    public string VectorJson { get; set; }
    public DateTime LastUpdatedOnUtc { get; set; }
}
