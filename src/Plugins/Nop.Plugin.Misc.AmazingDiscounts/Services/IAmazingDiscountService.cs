using Nop.Core;
using Nop.Plugin.Misc.AmazingDiscounts.Domain;

namespace Nop.Plugin.Misc.AmazingDiscounts.Services;

public partial interface IAmazingDiscountService
{
    Task<IList<AmazingDiscountProduct>> GetActiveAmazingDiscountProductsAsync();
    Task<IPagedList<AmazingDiscountProduct>> GetAllAmazingDiscountProductsAsync(int pageIndex = 0, int pageSize = int.MaxValue);
    Task<AmazingDiscountProduct> GetByIdAsync(int id);
    Task InsertAsync(AmazingDiscountProduct product);
    Task UpdateAsync(AmazingDiscountProduct product);
    Task DeleteAsync(AmazingDiscountProduct product);
}
