# Integrate AvalAI API Credit Checking and Threshold Warning

This plan describes how we will integrate the remaining API credit checks into the AvalAI AI plugin and display a warning to the administrator when the credit balance drops below a configurable threshold.

## User Review Required

> [!NOTE]
> System warnings are hooked using the `SystemWarningCreatedEvent` publisher. This allows warnings to automatically bubble up to the main administration dashboard and warnings page of nopCommerce, matching the requirement of alerting administrators system-wide.
> We will also show the current remaining credit and check warnings on the plugin's configuration page for immediate visibility.

## Open Questions

*None at this stage. The API response schema was verified via a live cURL test using the provided API key, confirming the structure of nested grants and packages.*

---

## Proposed Changes

### Configuration and Settings

#### [MODIFY] [AiSettings.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.ArtificialIntelligence/Domain/AiSettings.cs)
- Add a new property `CreditThreshold` (decimal, defaulting to `30000m` representing 30,000 Tomans).

#### [MODIFY] [AiSettingsModel.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.ArtificialIntelligence/Models/AiSettingsModel.cs)
- Add `CreditThreshold` (decimal).
- Add `CurrentCredit` (decimal?) to pass the current credit balance to the configuration view.

#### [MODIFY] [Configure.cshtml](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.ArtificialIntelligence/Views/Admin/Configure.cshtml)
- Add a field for the administrator to customize the credit threshold.
- Display the current credit balance and a custom warning label if it is below the threshold.

---

### API Client

#### [NEW] [AvalAiCreditResponse.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.ArtificialIntelligence/Services/AvalAiCreditResponse.cs)
- Create strongly typed response models to deserialize the JSON response of `https://api.avalai.ir/user/v1/credit`.

#### [MODIFY] [IAvalAiClient.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.ArtificialIntelligence/Services/IAvalAiClient.cs)
- Declare `Task<AvalAiCreditResponse> GetCreditAsync(string apiKey, string baseUrl);`.

#### [MODIFY] [AvalAiClient.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.ArtificialIntelligence/Services/AvalAiClient.cs)
- Implement `GetCreditAsync`. It resolves the absolute endpoint relative to the provided base URL and authenticates using the Bearer token.

---

### Event Consumer and Warnings

#### [MODIFY] [Nop.Plugin.Misc.ArtificialIntelligence.csproj](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.ArtificialIntelligence/Nop.Plugin.Misc.ArtificialIntelligence.csproj)
- Add a ProjectReference to `src/Presentation/Nop.Web/Nop.Web.csproj` to support consuming the system warning event.

#### [NEW] [SystemWarningConsumer.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.ArtificialIntelligence/Services/SystemWarningConsumer.cs)
- Create an event consumer implementing `IConsumer<SystemWarningCreatedEvent>` to verify credit balance and add warnings to the system dashboard when balance falls below the threshold.

#### [MODIFY] [AiAdminController.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.ArtificialIntelligence/Controllers/AiAdminController.cs)
- In the `Configure` GET action, load the settings, call the credit client, calculate the total credit (wallet balance + grants + packages), and display a warning if necessary.
- In the `Configure` POST action, save the updated `CreditThreshold`.

#### [MODIFY] [ArtificialIntelligencePlugin.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.ArtificialIntelligence/ArtificialIntelligencePlugin.cs)
- Add localization resource strings for warnings in both English and Persian.

---

## Verification Plan

### Automated Tests
- Run `dotnet build` to ensure the project compiles with no warnings/errors.

### Manual Verification
- Open the AI Settings page.
- Verify the current credit balance displays.
- Set a threshold higher than the remaining credit and verify that a warning notification is displayed.
- Set a threshold lower than the remaining credit and verify that no warning notification is displayed.
- Check the general system warnings dashboard page `/Admin/Common/Warnings` or the admin home page to ensure the warning is shown.
