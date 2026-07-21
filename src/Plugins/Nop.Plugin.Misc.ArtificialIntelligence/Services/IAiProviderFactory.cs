using Nop.Plugin.Misc.ArtificialIntelligence.Domain;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Services;

public interface IAiProviderFactory
{
    IAiClient GetClient(AiSettings settings);
}
