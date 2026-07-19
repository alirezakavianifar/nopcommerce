using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.MultiFactorAuth.SMS.Services;

namespace Nop.Plugin.MultiFactorAuth.SMS.Infrastructure;

/// <summary>
/// Represents object for configuring services on application startup
/// </summary>
public class NopStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // register custom services
        services.AddScoped<ISMSService, SMSService>();
        services.AddScoped<ICustomerSecurityRestrictionService, CustomerSecurityRestrictionService>();

        // Register custom action filter globally for IP/Device binding checks
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<ValidateCustomerSecurityRestrictionFilter>();
        });
    }

    /// <summary>
    /// Configure the using of added middleware
    /// </summary>
    public void Configure(IApplicationBuilder application)
    {
    }

    /// <summary>
    /// Gets order of this startup configuration implementation
    /// </summary>
    public int Order => 3001;
}
