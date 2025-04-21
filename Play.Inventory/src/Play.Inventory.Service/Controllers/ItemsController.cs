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
        // change from CatalogClient to IRepository<CatalogItem> after the rabbitMq consumer work
        private readonly IRepository<CatalogItem> _catalogRepository;
        public ItemsController(IRepository<InventoryItem> itemRepository,
                                IRepository<CatalogItem> catalogClient)
        {
            _itemRepository = itemRepository;
            _catalogRepository = catalogClient;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }
            var inventoryItemEntities = await _itemRepository.GetAllAsync(i => i.UserId == userId);

            var itemIds = inventoryItemEntities.Select(x => x.CatalogItemId);
            var catalogItemEntities = await _catalogRepository.GetAllAsync(c => itemIds.Contains(c.Id));

            var inventoryItemsDto = inventoryItemEntities.Select(inventoryItem =>
            {
                var catalogItem = catalogItemEntities.Single(catalog => catalog.Id == inventoryItem.CatalogItemId);
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