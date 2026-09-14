using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.GroupPurchase.Domain;

namespace Nop.Plugin.Misc.GroupPurchase.Services;

/// <summary>
/// Commission service for resolving gross profit commissions based on product, category, vendor, brand, or cost fallback
/// </summary>
public interface ICommissionService
{
    /// <summary>
    /// Resolves commission percentage based on priority: Product -> Category -> Vendor -> Manufacturer -> External -> null
    /// </summary>
    /// <param name="product">The product</param>
    /// <returns>Commission percentage or null if fallback to cost should be used</returns>
    Task<decimal?> ResolveCommissionPercentageAsync(Product product);

    /// <summary>
    /// Gets commission by identifier
    /// </summary>
    /// <param name="id">Commission identifier</param>
    /// <returns>Commission entity</returns>
    Task<GroupPurchaseCommission> GetCommissionByIdAsync(int id);

    /// <summary>
    /// Gets paged list of commissions
    /// </summary>
    Task<IPagedList<GroupPurchaseCommission>> GetAllCommissionsAsync(CommissionScope? scope = null, int pageIndex = 0, int pageSize = int.MaxValue);

    /// <summary>
    /// Inserts a new commission
    /// </summary>
    Task InsertCommissionAsync(GroupPurchaseCommission commission);

    /// <summary>
    /// Updates an existing commission
    /// </summary>
    Task UpdateCommissionAsync(GroupPurchaseCommission commission);

    /// <summary>
    /// Deletes a commission
    /// </summary>
    Task DeleteCommissionAsync(GroupPurchaseCommission commission);

    /// <summary>
    /// Imports commissions from an Excel or CSV file stream
    /// </summary>
    Task<(int importedCount, int errorCount)> ImportFromExcelOrCsvAsync(Stream fileStream);
}
