using ClosedXML.Excel;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Misc.GroupPurchase.Domain;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.GroupPurchase.Services;

/// <summary>
/// Implements commission resolution with the strict priority:
/// 1. Product Commission
/// 2. Category Commission
/// 3. Vendor Commission
/// 4. Manufacturer/Brand Commission
/// 5. External Provider (e.g. Foroush Gostar)
/// 6. Fallback to null (which triggers cost-based calculation: Price - ProductCost)
/// </summary>
public class CommissionService : ICommissionService
{
    private readonly IRepository<GroupPurchaseCommission> _commissionRepository;
    private readonly ICategoryService _categoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IExternalCommissionProvider _externalCommissionProvider;

    public CommissionService(
        IRepository<GroupPurchaseCommission> commissionRepository,
        ICategoryService categoryService,
        IManufacturerService manufacturerService,
        IExternalCommissionProvider externalCommissionProvider)
    {
        _commissionRepository = commissionRepository;
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
        _externalCommissionProvider = externalCommissionProvider;
    }

    /// <summary>
    /// Resolves commission percentage based on strict priority
    /// </summary>
    public virtual async Task<decimal?> ResolveCommissionPercentageAsync(Product product)
    {
        if (product == null)
            return null;

        var allCommissions = await _commissionRepository.GetAllAsync(query => query);

        // 1. Priority 1: Product specific commission
        var productComm = allCommissions.FirstOrDefault(c => 
            c.CommissionScopeId == (int)CommissionScope.Product && c.EntityId == product.Id);
        if (productComm != null)
            return productComm.Percentage;

        // 2. Priority 2: Category commission
        var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
        if (productCategories != null && productCategories.Any())
        {
            var categoryIds = productCategories.Select(pc => pc.CategoryId).ToList();
            var categoryComm = allCommissions.FirstOrDefault(c => 
                c.CommissionScopeId == (int)CommissionScope.Category && categoryIds.Contains(c.EntityId));
            if (categoryComm != null)
                return categoryComm.Percentage;
        }

        // 3. Priority 3: Vendor commission
        if (product.VendorId > 0)
        {
            var vendorComm = allCommissions.FirstOrDefault(c => 
                c.CommissionScopeId == (int)CommissionScope.Vendor && c.EntityId == product.VendorId);
            if (vendorComm != null)
                return vendorComm.Percentage;
        }

        // 4. Priority 4: Brand / Manufacturer commission
        var productManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id);
        if (productManufacturers != null && productManufacturers.Any())
        {
            var manufacturerIds = productManufacturers.Select(pm => pm.ManufacturerId).ToList();
            var mfgComm = allCommissions.FirstOrDefault(c => 
                c.CommissionScopeId == (int)CommissionScope.Manufacturer && manufacturerIds.Contains(c.EntityId));
            if (mfgComm != null)
                return mfgComm.Percentage;
        }

        // 5. Check external provider (Foroush Gostar sync)
        var externalComm = await _externalCommissionProvider.GetExternalCommissionPercentageAsync(product.Id, product.VendorId);
        if (externalComm.HasValue)
            return externalComm.Value;

        // 6. Return null -> fallback to ProductCost calculation
        return null;
    }

    public virtual async Task<GroupPurchaseCommission> GetCommissionByIdAsync(int id)
    {
        return await _commissionRepository.GetByIdAsync(id);
    }

    public virtual async Task<IPagedList<GroupPurchaseCommission>> GetAllCommissionsAsync(CommissionScope? scope = null, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = _commissionRepository.Table;
        if (scope.HasValue)
        {
            query = query.Where(c => c.CommissionScopeId == (int)scope.Value);
        }

        query = query.OrderBy(c => c.CommissionScopeId).ThenBy(c => c.EntityId);
        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public virtual async Task InsertCommissionAsync(GroupPurchaseCommission commission)
    {
        if (commission == null)
            throw new ArgumentNullException(nameof(commission));

        commission.CreatedOnUtc = DateTime.UtcNow;
        commission.UpdatedOnUtc = DateTime.UtcNow;

        await _commissionRepository.InsertAsync(commission);
    }

    public virtual async Task UpdateCommissionAsync(GroupPurchaseCommission commission)
    {
        if (commission == null)
            throw new ArgumentNullException(nameof(commission));

        commission.UpdatedOnUtc = DateTime.UtcNow;

        await _commissionRepository.UpdateAsync(commission);
    }

    public virtual async Task DeleteCommissionAsync(GroupPurchaseCommission commission)
    {
        if (commission == null)
            throw new ArgumentNullException(nameof(commission));

        await _commissionRepository.DeleteAsync(commission);
    }

    /// <summary>
    /// Imports commissions from Excel workbook (.xlsx)
    /// Expected format: Column 1: Scope (Product, Category, Vendor, Manufacturer), Column 2: EntityId, Column 3: Percentage, Column 4: Optional Name
    /// </summary>
    public virtual async Task<(int importedCount, int errorCount)> ImportFromExcelOrCsvAsync(Stream fileStream)
    {
        int importedCount = 0;
        int errorCount = 0;

        try
        {
            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                return (0, 0);

            var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header row
            foreach (var row in rows)
            {
                try
                {
                    var scopeText = row.Cell(1).GetString()?.Trim();
                    var entityIdVal = row.Cell(2).GetValue<int>();
                    var percentageVal = row.Cell(3).GetValue<decimal>();
                    var entityName = row.Cell(4).GetString()?.Trim() ?? string.Empty;

                    if (!Enum.TryParse<CommissionScope>(scopeText, true, out var scope))
                    {
                        errorCount++;
                        continue;
                    }

                    var existing = (await _commissionRepository.GetAllAsync(q =>
                        q.Where(c => c.CommissionScopeId == (int)scope && c.EntityId == entityIdVal))).FirstOrDefault();

                    if (existing != null)
                    {
                        existing.Percentage = percentageVal;
                        existing.EntityName = !string.IsNullOrEmpty(entityName) ? entityName : existing.EntityName;
                        existing.UpdatedOnUtc = DateTime.UtcNow;
                        await _commissionRepository.UpdateAsync(existing);
                    }
                    else
                    {
                        var newComm = new GroupPurchaseCommission
                        {
                            CommissionScopeId = (int)scope,
                            EntityId = entityIdVal,
                            Percentage = percentageVal,
                            EntityName = entityName,
                            CreatedOnUtc = DateTime.UtcNow,
                            UpdatedOnUtc = DateTime.UtcNow
                        };
                        await _commissionRepository.InsertAsync(newComm);
                    }

                    importedCount++;
                }
                catch
                {
                    errorCount++;
                }
            }
        }
        catch
        {
            errorCount++;
        }

        return (importedCount, errorCount);
    }
}
