using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.AmazingDiscounts.Services;

namespace Nop.Plugin.Misc.AmazingDiscounts.Infrastructure;

public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAmazingDiscountService, AmazingDiscountService>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 1;
}
