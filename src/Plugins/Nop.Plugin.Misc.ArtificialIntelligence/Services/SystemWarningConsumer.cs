using Nop.Services.Events;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Plugin.Misc.ArtificialIntelligence.Domain;
using Nop.Services.Configuration;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Services;

public class SystemWarningConsumer : IConsumer<SystemWarningCreatedEvent>
{
    private readonly IAvalAiClient _avalAiClient;
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;

    public SystemWarningConsumer(
        IAvalAiClient avalAiClient,
        ISettingService settingService,
        ILocalizationService localizationService)
    {
        _avalAiClient = avalAiClient;
        _settingService = settingService;
        _localizationService = localizationService;
    }

    public async Task HandleEventAsync(SystemWarningCreatedEvent eventMessage)
    {
        var settings = await _settingService.LoadSettingAsync<AiSettings>();
        if (settings == null)
            return;

        decimal totalRemainingIrt = 0;
        bool isLow = false;

        if (settings.SandboxMode)
        {
            totalRemainingIrt = 125000m; // Mock balance for sandbox mode
            isLow = totalRemainingIrt <= settings.CreditThreshold;
        }
        else if (!string.IsNullOrEmpty(settings.ApiKey))
        {
            var creditInfo = await _avalAiClient.GetCreditAsync(settings.ApiKey, settings.BaseUrl);
            if (creditInfo != null)
            {
                totalRemainingIrt = creditInfo.RemainingIrt;
                if (creditInfo.CreditSources?.Grants != null)
                {
                    foreach (var grant in creditInfo.CreditSources.Grants)
                    {
                        if (decimal.TryParse(grant.RemainingIrt, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var grantVal))
                        {
                            totalRemainingIrt += grantVal;
                        }
                    }
                }
                if (creditInfo.CreditSources?.Packages != null)
                {
                    foreach (var package in creditInfo.CreditSources.Packages)
                    {
                        if (decimal.TryParse(package.RemainingIrt, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var pkgVal))
                        {
                            totalRemainingIrt += pkgVal;
                        }
                    }
                }

                isLow = totalRemainingIrt <= settings.CreditThreshold;
            }
        }

        if (isLow)
        {
            var warningFormat = await _localizationService.GetResourceAsync("Plugins.Misc.ArtificialIntelligence.CreditWarning");
            if (string.IsNullOrEmpty(warningFormat) || warningFormat.Equals("Plugins.Misc.ArtificialIntelligence.CreditWarning"))
            {
                warningFormat = "AvalAI credit is low. Remaining credit is {0} Tomans, which is below the threshold of {1} Tomans.";
            }

            var text = string.Format(warningFormat, totalRemainingIrt.ToString("N0"), settings.CreditThreshold.ToString("N0"));
            if (settings.SandboxMode)
            {
                text = $"[Sandbox] {text}";
            }

            eventMessage.SystemWarnings.Add(new SystemWarningModel
            {
                Level = SystemWarningLevel.Warning,
                DontEncode = false,
                Text = text
            });
        }
    }
}
