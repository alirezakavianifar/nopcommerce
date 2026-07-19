using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Events;
using Nop.Plugin.Misc.ArtificialIntelligence.Domain;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Services;

public class ProductRegistrationConsumer : IConsumer<EntityInsertedEvent<Product>>, IConsumer<EntityUpdatedEvent<Product>>
{
    private readonly IAiService _aiService;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<AiDuplicateProductQueue> _duplicateQueueRepository;

    public ProductRegistrationConsumer(
        IAiService aiService,
        IRepository<Product> productRepository,
        IRepository<AiDuplicateProductQueue> duplicateQueueRepository)
    {
        _aiService = aiService;
        _productRepository = productRepository;
        _duplicateQueueRepository = duplicateQueueRepository;
    }

    public async Task HandleEventAsync(EntityInsertedEvent<Product> eventMessage)
    {
        var product = eventMessage.Entity;
        if (product == null)
            return;

        await CheckAndProcessDuplicateAsync(product);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
    {
        var product = eventMessage.Entity;
        if (product == null)
            return;

        await CheckAndProcessDuplicateAsync(product);
    }

    private async Task CheckAndProcessDuplicateAsync(Product product)
    {
        // Prevent checking products that are deleted
        if (product.Deleted)
            return;

        // Prevent infinite loops: check if this product has already been analyzed/queued
        var existingQueueItem = (await _duplicateQueueRepository.GetAllAsync(query =>
            query.Where(q => q.ProductId == product.Id)
        )).FirstOrDefault();

        if (existingQueueItem != null)
        {
            // If it is already in the queue, we respect the current approval/rejection state.
            // If it was rejected, ensure it remains unpublished.
            if (existingQueueItem.Status == DuplicateStatus.Rejected && product.Published)
            {
                product.Published = false;
                await _productRepository.UpdateAsync(product);
            }
            return;
        }

        // Run the AI semantic duplicate check
        var checkResult = await _aiService.CheckDuplicateAsync(product.Id);

        if (checkResult.IsDuplicate)
        {
            // Add the entry to the admin queue first to prevent nested events from checking duplicates again
            var queueItem = new AiDuplicateProductQueue
            {
                ProductId = product.Id,
                VendorId = product.VendorId,
                DuplicateProductId = checkResult.DuplicateProductId,
                Status = DuplicateStatus.Pending,
                Explanation = string.Empty,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _duplicateQueueRepository.InsertAsync(queueItem);

            // Block the product from storefront publication if it is currently published
            if (product.Published)
            {
                product.Published = false;
                await _productRepository.UpdateAsync(product);
            }
        }
    }
}
