using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Contracts;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Common.Repositories;

namespace Play.Catalog.Service.Controllers;

[Route("items")]
[ApiController]
public class ItemsController : ControllerBase
{

    private readonly IRepository<Item> _itemRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    public ItemsController(IRepository<Item> itemRepository,
                            IPublishEndpoint publishEndpoint)
    {
        _itemRepository = itemRepository;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetAllAsync()
    {
        var items = (await _itemRepository.GetAllAsync())
                    .Select(i => i.ToDto());

        return Ok(items);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var item = await _itemRepository.GetByIdAsync(id);
        if (item == null)
        {
            return NotFound();
        }
        return Ok(item.ToDto());
    }
    [HttpPost]
    public async Task<ActionResult> PostAsync(AddItemDto itemDto)
    {
        Item itemToAdd = new Item
        {
            Id = Guid.NewGuid(),
            Name = itemDto.Name,
            Description = itemDto.Description,
            Price = itemDto.Price,
            CreatedDate = DateTimeOffset.Now
        };
        await _itemRepository.CreateAsync(itemToAdd);

        await _publishEndpoint.Publish(new CatalogItemCreated(itemToAdd.Id, itemToAdd.Name, itemToAdd.Description));

        return CreatedAtAction(nameof(GetByIdAsync), new { id = itemToAdd.Id }, itemToAdd);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto itemDto)
    {
        Item? itemToUpdate = await _itemRepository.GetByIdAsync(id);
        if (itemToUpdate == null)
        {
            return NotFound();
        }
        itemToUpdate.Name = itemDto.Name;
        itemToUpdate.Description = itemDto.Description;
        itemToUpdate.Price = itemDto.Price;
        await _itemRepository.UpdateAsync(itemToUpdate);

        await _publishEndpoint.Publish(new CatalogItemUpdated(itemToUpdate.Id, itemToUpdate.Name, itemToUpdate.Description));

        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var item = await _itemRepository.GetByIdAsync(id);

        if (item == null)
        {
            return NotFound();
        }
        await _itemRepository.DeleteAsync(item.Id);

        await _publishEndpoint.Publish(new CatalogItemDeleted(item.Id));

        return NoContent();
    }
}