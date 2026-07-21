using Nop.Plugin.Misc.ArtificialIntelligence.Domain;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Services;

public class AiProviderFactory : IAiProviderFactory
{
    private readonly AvalAiClient _avalAiClient;
    private readonly LocalAiClient _localAiClient;

    public AiProviderFactory(AvalAiClient avalAiClient, LocalAiClient localAiClient)
    {
        _avalAiClient = avalAiClient;
        _localAiClient = localAiClient;
    }

    public IAiClient GetClient(AiSettings settings)
    {
        if (settings == null)
            return _avalAiClient;

        if (settings.SandboxMode || settings.ProviderType == AiProviderType.Sandbox)
        {
            // Sandbox is handled in AiService or returning AvalAiClient with sandbox checks
            return _avalAiClient;
        }

        switch (settings.ProviderType)
        {
            case AiProviderType.LocalInfrastructure:
                return _localAiClient;

            case AiProviderType.CloudAvalAi:
            default:
                return _avalAiClient;
        }
    }
}
