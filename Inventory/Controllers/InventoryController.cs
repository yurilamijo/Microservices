﻿using GenericRepository.Repositories;
using Inventory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Contracts;
using MassTransit;
using static Inventory.Dtos;

namespace Inventory.Controllers
{
    [ApiController]
    [Route("items")]
    public class InventoryController : ControllerBase
    {
        private const string AdminRole = "Admin";

        private readonly IRepository<InventoryItem> _inventoryRepository;
        private readonly IRepository<CatalogItem> _catalogItemRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public InventoryController(
            IRepository<InventoryItem> inventoryRepository, 
            IRepository<CatalogItem> catalogItemRepository, 
            IPublishEndpoint publishEndpoint)
        {
            _inventoryRepository = inventoryRepository;
            _catalogItemRepository = catalogItemRepository;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            // Get subclaim
            var currentUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!Guid.Parse(currentUserId).Equals(userId))
            {
                if (!User.IsInRole(AdminRole))
                {
                    return Forbid();
                }
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
        [Authorize(Roles = AdminRole)]
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

            await _publishEndpoint.Publish(new InventoryContracts.InventoryItemUpdated(inventoryItem.UserId, inventoryItem.CatalogItemId, inventoryItem.Quantity));

            return Ok();
        }
    }
}
