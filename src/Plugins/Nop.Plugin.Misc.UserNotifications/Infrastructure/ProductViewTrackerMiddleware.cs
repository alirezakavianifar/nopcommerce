using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Misc.UserNotifications.Domain;
using Nop.Plugin.Misc.UserNotifications.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Seo;

namespace Nop.Plugin.Misc.UserNotifications.Infrastructure;

public class ProductViewTrackerMiddleware
{
    private readonly RequestDelegate _next;

    public ProductViewTrackerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IWorkContext workContext,
        ICustomerService customerService,
        IProductService productService,
        IUrlRecordService urlRecordService,
        IRepository<ProductViewLog> logRepository,
        IWorkflowEngineService workflowEngineService)
    {
        await _next(context);

        try
        {
            var customer = await workContext.GetCurrentCustomerAsync();
            if (customer != null && !await customerService.IsGuestAsync(customer))
            {
                var path = context.Request.Path.Value?.TrimStart('/').ToLower();
                if (!string.IsNullOrWhiteSpace(path))
                {
                    var urlRecord = await urlRecordService.GetBySlugAsync(path);
                    if (urlRecord != null && urlRecord.EntityName == nameof(Product) && urlRecord.IsActive)
                    {
                        var product = await productService.GetProductByIdAsync(urlRecord.EntityId);
                        if (product != null)
                        {
                            var log = new ProductViewLog
                            {
                                CustomerId = customer.Id,
                                ProductId = product.Id,
                                ViewedOnUtc = DateTime.UtcNow
                            };
                            await logRepository.InsertAsync(log);

                            await workflowEngineService.TriggerWorkflowAsync(
                                NotificationTriggerType.ProductViewed,
                                customer.Id,
                                productId: product.Id);
                        }
                    }
                }
            }
        }
        catch
        {
            // Do not break main pipeline on tracking errors
        }
    }
}
