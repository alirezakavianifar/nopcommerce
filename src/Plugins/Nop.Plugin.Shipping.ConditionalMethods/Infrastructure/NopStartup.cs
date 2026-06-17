using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Shipping.ConditionalMethods.Services;

namespace Nop.Plugin.Shipping.ConditionalMethods.Infrastructure;

/// <summary>
/// Registers plugin services in the DI container
/// </summary>
public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IConditionalShippingService, ConditionalShippingService>();
        services.AddScoped<ICourierApiService, CourierApiService>();
        services.AddScoped<IFreightApiService, FreightApiService>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 1;
}
