using Microsoft.AspNetCore.Mvc;
using Play.Common.Repositories;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<InventoryItem> _itemRepository;
        private readonly CatalogClient _catalogClient;
        public ItemsController(IRepository<InventoryItem> itemRepository,
                                CatalogClient catalogClient)
        {
            _itemRepository = itemRepository;
            _catalogClient = catalogClient;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }
            var catalogItems = await _catalogClient.GetCatalogItemsAsync();
            var inventoryItemsEntity = await _itemRepository.GetAllAsync(i => i.UserId == userId);
            var inventoryItemsDto = inventoryItemsEntity.Select(inventoryItem =>
            {
                var catalogItem = catalogItems.Single(catalog => catalog.Id == inventoryItem.CatalogItemId);
                return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
            });
            return Ok(inventoryItemsDto);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(GrantItemDto grantItemDto)
        {
            InventoryItem? existedItem = await _itemRepository.GetAsync(i => i.CatalogItemId == grantItemDto.CatalogItemId && i.UserId == grantItemDto.UserId);
            if (existedItem != null)
            {
                existedItem.Quantity += grantItemDto.Quantity;
                await _itemRepository.UpdateAsync(existedItem);
            }
            else
            {
                InventoryItem item = new InventoryItem
                {
                    Id = Guid.NewGuid(),
                    UserId = grantItemDto.UserId,
                    CatalogItemId = grantItemDto.CatalogItemId,
                    Quantity = grantItemDto.Quantity,
                    AcquiredDate = DateTimeOffset.Now
                };
                await _itemRepository.CreateAsync(item);
            }
            return Ok();
        }
    }
}