using GenericRepository.Repositories;
using Inventory.Clients;
using Inventory.Models;
using Microsoft.AspNetCore.Mvc;
using static Inventory.Dtos;

namespace Inventory.Controllers
{
    [ApiController]
    [Route("items")]
    public class InventoryController : ControllerBase
    {
        private readonly IRepository<InventoryItem> _inventoryRepository;
        private readonly CatalogClient _catalogClient;

        public InventoryController(IRepository<InventoryItem> inventoryRepository, CatalogClient catalogClient)
        {
            _inventoryRepository = inventoryRepository;
            _catalogClient = catalogClient;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            var catalogItems = await _catalogClient.GetCatalogItemsAsync();
            var inventoryItemsEntities = await _inventoryRepository.GetAllAsync(item => item.UserId == userId);

            var inventoryItemsDtos = inventoryItemsEntities.Select(inventoryItem =>
            {
                var catalogItem = catalogItems.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
                return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
            });

            return Ok(inventoryItemsDtos);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(GrantItemsDto itemsDto)
        {
            var inventoryItem = await _inventoryRepository.GetAsync(
                item => item.UserId == itemsDto.UserId
                && item.CatalogItemId == itemsDto.CatalogItemId
            );

            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = itemsDto.CatalogItemId,
                    UserId = itemsDto.UserId,
                    Quantity = itemsDto.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };

                await _inventoryRepository.CreateAsync(inventoryItem);
            }
            else
            {
                // If item was found
                inventoryItem.Quantity += itemsDto.Quantity;
                await _inventoryRepository.UpdateAsync(inventoryItem);
            }

            return Ok();
        }
    }
}
