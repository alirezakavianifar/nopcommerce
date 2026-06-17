# Implementation Plan - Phase 2: Group Purchase Basic

Implement the core logic for the Group Purchase system, allowing customers to convert their shopping cart into a group purchase and other customers to join the group via a unique code.

## Proposed Changes

### [Component] Group Purchase Plugin Setup

#### [NEW] [Nop.Plugin.Misc.GroupPurchase.csproj](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.GroupPurchase/Nop.Plugin.Misc.GroupPurchase.csproj)
Define the project file for the Group Purchase plugin, targeting .NET 9.0 and including necessary nopCommerce references.

#### [NEW] [plugin.json](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.GroupPurchase/plugin.json)
Configure the plugin metadata.

#### [NEW] [GroupPurchasePlugin.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.GroupPurchase/GroupPurchasePlugin.cs)
Implement the main plugin class, including install/uninstall logic.

---

### [Component] Domain & Data

#### [NEW] [GroupPurchase.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.GroupPurchase/Domain/GroupPurchase.cs)
Domain model for the group purchase session.

#### [NEW] [GroupPurchaseMember.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.GroupPurchase/Domain/GroupPurchaseMember.cs)
Domain model for group purchase participants.

#### [NEW] [GroupPurchaseStatus.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.GroupPurchase/Domain/GroupPurchaseStatus.cs)
Enum for group purchase statuses (Active, Completed, Cancelled).

#### [NEW] [VisibilityType.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.GroupPurchase/Domain/VisibilityType.cs)
Enum for member visibility rules.

#### [NEW] [GroupPurchaseMigration.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.GroupPurchase/Data/GroupPurchaseMigration.cs)
Database migration to create the `GroupPurchase` and `GroupPurchaseMember` tables.

---

### [Component] Services & Infrastructure

#### [NEW] [IGroupPurchaseService.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.GroupPurchase/Services/IGroupPurchaseService.cs)
#### [NEW] [GroupPurchaseService.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.GroupPurchase/Services/GroupPurchaseService.cs)
Implement core logic:
- `CreateGroupPurchaseAsync(Customer leader)`
- `JoinGroupPurchaseAsync(Customer member, string uniqueCode)`
- `GetGroupPurchaseByCodeAsync(string uniqueCode)`

#### [NEW] [NopStartup.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Plugins/Nop.Plugin.Misc.GroupPurchase/Infrastructure/NopStartup.cs)
Register services in the DI container.

---

### [Component] UI & Logic (Basic)

#### [MODIFY] [IShoppingCartService.cs](file:///e:/projects/nopCommerce_4.90.3_Source/src/Libraries/Nop.Services/Orders/IShoppingCartService.cs) (Maybe?)
I'll check if I can use generic attributes on the customer/cart instead of modifying core.
> [!IMPORTANT]
> I will aim to use `IGenericAttributeService` to tag the customer's cart with a `GroupPurchaseId` instead of modifying the core `ShoppingCartItem` or `Order` entities directly, ensuring zero core modification.

## Verification Plan

### Automated Tests
- I will create unit tests for `GroupPurchaseService` in a new test project or within the plugin.
- Command: `dotnet test src\Plugins\Nop.Plugin.Misc.GroupPurchase` (after setting up tests)

### Manual Verification
- Install the plugin via Admin panel.
- Verify table creation in the database.
- Use a test tool (or temporary controller endpoint) to trigger "Convert to Group Purchase" and verify the `GroupPurchase` record and its unique code.
- Use another customer account to "Join" using the code and verify `GroupPurchaseMember` record.
