using GenericRepository.Repositories;
using Inventory.Exceptions;
using Inventory.Models;
using MassTransit;
using static Contracts.InventoryContracts;

namespace Inventory.Consumers
{
    public class SubtractItemsConsumer : IConsumer<SubstractItems>
    {
        private readonly IRepository<InventoryItem> _inventoryRepository;
        private readonly IRepository<CatalogItem> _catalogItemRepository;

        public SubtractItemsConsumer(IRepository<InventoryItem> inventoryRepository, IRepository<CatalogItem> catalogItemRepository)
        {
            _inventoryRepository = inventoryRepository;
            _catalogItemRepository = catalogItemRepository;
        }

        public async Task Consume(ConsumeContext<SubstractItems> context)
        {
            var message = context.Message;

            var item = await _catalogItemRepository.GetAsync(message.CatalogItemId);

            if (item == null)
            {
                throw new UnknownItemException(message.CatalogItemId);
            }

            var inventoryItem = await _inventoryRepository.GetAsync(
                item => item.UserId == message.UserId
                && item.CatalogItemId == message.CatalogItemId
            );

            if (inventoryItem != null)
            {
                if (inventoryItem.MessageIds.Contains(context.MessageId.Value))
                {
                    await context.Publish(new InventoryItemsSubstracted(message.CorrelationId));
                    return;
                }

                inventoryItem.Quantity -= message.Quantity;
                inventoryItem.MessageIds.Add(context.MessageId.Value);
                await _inventoryRepository.UpdateAsync(inventoryItem);
            }

            await context.Publish(new InventoryItemsSubstracted(message.CorrelationId));
        }
    }
}
