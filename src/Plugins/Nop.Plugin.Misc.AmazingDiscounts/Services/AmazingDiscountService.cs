using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.AmazingDiscounts.Domain;

namespace Nop.Plugin.Misc.AmazingDiscounts.Services;

public partial class AmazingDiscountService : IAmazingDiscountService
{
    protected readonly IRepository<AmazingDiscountProduct> _repository;

    public AmazingDiscountService(IRepository<AmazingDiscountProduct> repository)
    {
        _repository = repository;
    }

    public virtual async Task<IList<AmazingDiscountProduct>> GetActiveAmazingDiscountProductsAsync()
    {
        var nowUtc = DateTime.UtcNow;

        return await _repository.GetAllAsync(query =>
        {
            query = query.Where(p => !p.StartDateUtc.HasValue || p.StartDateUtc <= nowUtc);
            query = query.Where(p => !p.EndDateUtc.HasValue || p.EndDateUtc >= nowUtc);
            query = query.OrderBy(p => p.DisplayOrder);

            return query;
        });
    }

    public virtual async Task<IPagedList<AmazingDiscountProduct>> GetAllAmazingDiscountProductsAsync(int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _repository.GetAllPagedAsync(query => query.OrderBy(p => p.DisplayOrder), pageIndex, pageSize);
    }

    public virtual async Task<AmazingDiscountProduct> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public virtual async Task InsertAsync(AmazingDiscountProduct product)
    {
        await _repository.InsertAsync(product);
    }

    public virtual async Task UpdateAsync(AmazingDiscountProduct product)
    {
        await _repository.UpdateAsync(product);
    }

    public virtual async Task DeleteAsync(AmazingDiscountProduct product)
    {
        await _repository.DeleteAsync(product);
    }
}
