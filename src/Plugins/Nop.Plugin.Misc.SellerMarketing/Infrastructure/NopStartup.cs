using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.SellerMarketing.Services;

namespace Nop.Plugin.Misc.SellerMarketing.Infrastructure;

public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISellerMarketingService, SellerMarketingService>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 1;
}
