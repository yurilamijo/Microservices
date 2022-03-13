﻿using GenericRepository.Repositories;
using Inventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Inventory.Dtos;

namespace Inventory.Controllers
{
    [ApiController]
    [Route("items")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IRepository<InventoryItem> _inventoryRepository;
        private readonly IRepository<CatalogItem> _catalogItemRepository;

        public InventoryController(IRepository<InventoryItem> inventoryRepository, IRepository<CatalogItem> catalogItemRepository)
        {
            _inventoryRepository = inventoryRepository;
            _catalogItemRepository = catalogItemRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            // Get inventory items of user
            var inventoryItemsEntities = await _inventoryRepository.GetAllAsync(item => item.UserId == userId);
            
            // Get ids of inventory items
            var itemIds = inventoryItemsEntities.Select(item => item.CatalogItemId);
            
            // Get catalog item data
            var catalogItemEntities = await _catalogItemRepository.GetAllAsync(item => itemIds.Contains(item.Id));

            var inventoryItemsDtos = inventoryItemsEntities.Select(inventoryItem =>
            {
                var catalogItem = catalogItemEntities.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
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
