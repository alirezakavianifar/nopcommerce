using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Nop.Plugin.Misc.ArtificialIntelligence.Domain;
using Nop.Plugin.Misc.ArtificialIntelligence.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Common;
using NUnit.Framework;

namespace Nop.Tests.Nop.Web.Tests.Events;

[TestFixture]
public class CreditCheckingTests
{
    [Test]
    public void AvalAiCreditResponse_CanBeDeserializedCorrectly()
    {
        // Arrange
        var jsonString = @"
        {
            ""limit"": 1000000.0,
            ""remaining_irt"": 50000.0,
            ""remaining_unit"": 5.0,
            ""total_unit"": 100.0,
            ""exchange_rate"": 10000.0,
            ""account_tier"": 2,
            ""credit_sources"": {
                ""grants"": [
                    {
                        ""id"": ""g1"",
                        ""description"": ""Welcome Grant"",
                        ""amount_irt"": ""15000.00"",
                        ""remaining_irt"": ""10000.00"",
                        ""end_date"": ""2026-12-31""
                    }
                ],
                ""packages"": [
                    {
                        ""id"": ""p1"",
                        ""description"": ""Pro Package"",
                        ""amount_irt"": ""100000.00"",
                        ""remaining_irt"": ""80000.00"",
                        ""end_date"": ""2026-08-31""
                    }
                ]
            }
        }";

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // Act
        var response = JsonSerializer.Deserialize<AvalAiCreditResponse>(jsonString, options);

        // Assert
        response.Should().NotBeNull();
        response.Limit.Should().Be(1000000m);
        response.RemainingIrt.Should().Be(50000m);
        response.CreditSources.Should().NotBeNull();
        response.CreditSources.Grants.Should().HaveCount(1);
        response.CreditSources.Grants[0].Id.Should().Be("g1");
        response.CreditSources.Grants[0].RemainingIrt.Should().Be("10000.00");
        response.CreditSources.Packages.Should().HaveCount(1);
        response.CreditSources.Packages[0].Id.Should().Be("p1");
        response.CreditSources.Packages[0].RemainingIrt.Should().Be("80000.00");
    }

    [Test]
    public async Task SystemWarningConsumer_SandboxMode_AddsWarning_WhenCreditIsBelowThreshold()
    {
        // Arrange
        var settings = new AiSettings
        {
            SandboxMode = true,
            CreditThreshold = 150000m // Sandbox default is 125,000m
        };

        var mockSettingService = new Mock<ISettingService>();
        mockSettingService.Setup(s => s.LoadSettingAsync<AiSettings>(It.IsAny<int>())).ReturnsAsync(settings);

        var mockLocalizationService = new Mock<ILocalizationService>();
        mockLocalizationService.Setup(l => l.GetResourceAsync("Plugins.Misc.ArtificialIntelligence.CreditWarning"))
            .ReturnsAsync("Low credit alert: {0} < {1}");

        var mockClient = new Mock<IAvalAiClient>();

        var consumer = new SystemWarningConsumer(mockClient.Object, mockSettingService.Object, mockLocalizationService.Object);
        var eventMessage = new SystemWarningCreatedEvent();

        // Act
        await consumer.HandleEventAsync(eventMessage);

        // Assert
        eventMessage.SystemWarnings.Should().HaveCount(1);
        eventMessage.SystemWarnings[0].Text.Should().Contain("[Sandbox] Low credit alert: 125,000 < 150,000");
    }

    [Test]
    public async Task SystemWarningConsumer_SandboxMode_NoWarning_WhenCreditIsAboveThreshold()
    {
        // Arrange
        var settings = new AiSettings
        {
            SandboxMode = true,
            CreditThreshold = 100000m // Sandbox default is 125,000m
        };

        var mockSettingService = new Mock<ISettingService>();
        mockSettingService.Setup(s => s.LoadSettingAsync<AiSettings>(It.IsAny<int>())).ReturnsAsync(settings);

        var mockLocalizationService = new Mock<ILocalizationService>();
        var mockClient = new Mock<IAvalAiClient>();

        var consumer = new SystemWarningConsumer(mockClient.Object, mockSettingService.Object, mockLocalizationService.Object);
        var eventMessage = new SystemWarningCreatedEvent();

        // Act
        await consumer.HandleEventAsync(eventMessage);

        // Assert
        eventMessage.SystemWarnings.Should().BeEmpty();
    }

    [Test]
    public async Task SystemWarningConsumer_LiveMode_AddsWarning_WhenSumOfCreditGrantsAndPackagesIsBelowThreshold()
    {
        // Arrange
        var settings = new AiSettings
        {
            SandboxMode = false,
            ApiKey = "test-api-key",
            BaseUrl = "https://api.avalai.ir/v1",
            CreditThreshold = 40000m
        };

        var mockSettingService = new Mock<ISettingService>();
        mockSettingService.Setup(s => s.LoadSettingAsync<AiSettings>(It.IsAny<int>())).ReturnsAsync(settings);

        var mockLocalizationService = new Mock<ILocalizationService>();
        mockLocalizationService.Setup(l => l.GetResourceAsync("Plugins.Misc.ArtificialIntelligence.CreditWarning"))
            .ReturnsAsync("Low credit: {0} < {1}");

        var creditResponse = new AvalAiCreditResponse
        {
            RemainingIrt = 10000m,
            CreditSources = new CreditSources
            {
                Grants = new List<CreditSourceDetail>
                {
                    new() { RemainingIrt = "5000.00" }
                },
                Packages = new List<CreditSourceDetail>
                {
                    new() { RemainingIrt = "15000.00" }
                }
            }
        };

        var mockClient = new Mock<IAvalAiClient>();
        mockClient.Setup(c => c.GetCreditAsync("test-api-key", "https://api.avalai.ir/v1"))
            .ReturnsAsync(creditResponse);

        var consumer = new SystemWarningConsumer(mockClient.Object, mockSettingService.Object, mockLocalizationService.Object);
        var eventMessage = new SystemWarningCreatedEvent();

        // Act
        await consumer.HandleEventAsync(eventMessage);

        // Assert
        // Total credit is 10,000 + 5,000 + 15,000 = 30,000 Tomans, which is <= 40,000 Tomans threshold.
        eventMessage.SystemWarnings.Should().HaveCount(1);
        eventMessage.SystemWarnings[0].Text.Should().Contain("Low credit: 30,000 < 40,000");
    }

    [Test]
    public async Task SystemWarningConsumer_LiveMode_NoWarning_WhenSumOfCreditGrantsAndPackagesIsAboveThreshold()
    {
        // Arrange
        var settings = new AiSettings
        {
            SandboxMode = false,
            ApiKey = "test-api-key",
            BaseUrl = "https://api.avalai.ir/v1",
            CreditThreshold = 25000m
        };

        var mockSettingService = new Mock<ISettingService>();
        mockSettingService.Setup(s => s.LoadSettingAsync<AiSettings>(It.IsAny<int>())).ReturnsAsync(settings);

        var mockLocalizationService = new Mock<ILocalizationService>();

        var creditResponse = new AvalAiCreditResponse
        {
            RemainingIrt = 10000m,
            CreditSources = new CreditSources
            {
                Grants = new List<CreditSourceDetail>
                {
                    new() { RemainingIrt = "5000.00" }
                },
                Packages = new List<CreditSourceDetail>
                {
                    new() { RemainingIrt = "15000.00" }
                }
            }
        };

        var mockClient = new Mock<IAvalAiClient>();
        mockClient.Setup(c => c.GetCreditAsync("test-api-key", "https://api.avalai.ir/v1"))
            .ReturnsAsync(creditResponse);

        var consumer = new SystemWarningConsumer(mockClient.Object, mockSettingService.Object, mockLocalizationService.Object);
        var eventMessage = new SystemWarningCreatedEvent();

        // Act
        await consumer.HandleEventAsync(eventMessage);

        // Assert
        // Total credit is 30,000 Tomans, which is > 25,000 Tomans threshold.
        eventMessage.SystemWarnings.Should().BeEmpty();
    }
}
