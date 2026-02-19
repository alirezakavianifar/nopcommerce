# nopCommerce 4.90 Plugin Development Guide

This guide outlines the best practices and lessons learned while building the **System Announcement Plugin**, providing a template for future modules like **Amazing Discounts** and **Group Purchase**.

---

## 🏗️ 1. Project Infrastructure

### **Target Framework**
Ensure all plugins target `.NET 9.0` to match the core solution:
```xml
<TargetFramework>net9.0</TargetFramework>
```

### **Critical Project References**
Always reference `Nop.Web.Framework`:
```xml
<ProjectReference Include="$(SolutionDir)\Presentation\Nop.Web.Framework\Nop.Web.Framework.csproj" />
```

### **Asset Deployment (Crucial)**
By default, `.cshtml` files are not copied to the output directory. You must explicitly include them in the `.csproj`:
```xml
<ItemGroup>
    <None Remove="plugin.json" />
    <None Remove="Views\**\*.cshtml" />
</ItemGroup>

<ItemGroup>
    <Content Include="plugin.json">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Include="Views\**\*.cshtml">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
</ItemGroup>
```

---

## 🗄️ 2. Data & Domain

### **Schema Migrations**
Use `FluentMigrator` (via `Nop.Data.Migrations`).
*   Inherit from `AutoReversingMigration`.
*   Maintain a clear naming convention: `{PluginName}Builder.cs` for table definitions.

### **Service Naming**
Avoid generic names like `INotificationService` to prevent conflicts with nopCommerce core. Use specific prefixes:
*   ✅ `IUserNotificationService`
*   ✅ `IAmazingDiscountService`

---

## 🛠️ 3. Backend Implementation (Controllers)

### **Admin Controller Requirements**
NopCommerce 4.90 requires specific attributes for security and routing:
```csharp
[AuthorizeAdmin]
[Area(AreaNames.ADMIN)] // Use the constant from Nop.Web.Framework
[AutoValidateAntiforgeryToken]
public class MyAdminController : BasePluginController { ... }
```

### **Security & Permissions**
Use the `[CheckPermission]` attribute instead of manual checks:
```csharp
[CheckPermission(StandardPermission.Configuration.ManagePlugins)]
public async Task<IActionResult> List() { ... }
```

---

## 🖥️ 4. UI & Admin Integration

### **Admin Menu Integration**
Implement `IAdminMenuPlugin`:
*   Use `rootNode.GetItemBySystemName("Catalog")` or `"Promotions"` to find parent nodes.
*   **Note**: This interface is officially obsolete in 4.90 but remains the most efficient way to inject custom menus. Future-proofing would involve `AdminMenuCreatedEvent`.

### **Admin Grids (DataTables)**
*   Use `DataTablesModel` for standardized grids.
*   **Namespace**: `@using Nop.Web.Framework.Models.DataTables` must be added to `_ViewImports.cshtml`.
*   **Columns**: Use `RenderDate()`, `RenderBoolean()`, and `RenderButtonEdit()` for standard interactions.

### **Public UI (Widgets)**
Implement `IWidgetPlugin`:
*   Identify the best `PublicWidgetZones` (e.g., `HomepageTop`, `HeaderBefore`).
*   Return a `ViewComponent` rather than a raw View for better performance and isolation.

---

## 🧪 5. Common Pitfalls to Avoid

1.  **Missing Namespace in Views**: If you get "Type not found" errors in a `.cshtml` file, check `_ViewImports.cshtml`.
2.  **Locked Files**: If `dotnet build` fails, ensure the `Nop.Web` process is stopped.
3.  **Invalid View Path**: Always use the full plugin path: `~/Plugins/Misc.UserNotifications/Views/Admin/List.cshtml`.
4.  **Integrated Auth**: When manually initializing the database, ensure your connection string uses `Integrated Security=True` for SQL Express.

---

## 🚀 6. Workflow for New Plugins

1.  **Scaffold**: Create folder and `.csproj`.
2.  **Domain**: Define `BaseEntity` and `Migration`.
3.  **Service**: Implement CRUD logic.
4.  **Admin UI**: Create Search/List Models -> Controller -> `List.cshtml`.
5.  **Public UI**: Implement `IWidgetPlugin` and `ViewComponent`.
6.  **Configuration**: Implement `GetConfigurationPageUrl` in `Plugin.cs`.
