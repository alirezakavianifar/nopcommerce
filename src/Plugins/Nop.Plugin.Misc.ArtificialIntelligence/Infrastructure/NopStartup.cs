using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.ArtificialIntelligence.Services;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Infrastructure;

public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AvalAiClient>();
        services.AddScoped<IAvalAiClient>(sp => sp.GetRequiredService<AvalAiClient>());
        services.AddScoped<LocalAiClient>();
        services.AddScoped<IAiProviderFactory, AiProviderFactory>();
        services.AddScoped<IAiService, AiService>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 1;
}
