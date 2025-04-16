using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Common.Repositories;

namespace Play.Catalog.Service.Controllers;

[Route("items")]
[ApiController]
public class ItemsController : ControllerBase
{

    private readonly IRepository<Item> _itemRepository;
    public ItemsController(IRepository<Item> itemRepository)
    {
        _itemRepository = itemRepository;
    }

    [HttpGet]
    public async Task<IEnumerable<ItemDto>> GetAllAsync()
    {
        var items = (await _itemRepository.GetAllAsync())
                    .Select(i => i.ToDto());
        return items;
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var item = await _itemRepository.GetByIdAsync(id);
        if (item == null) {
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
        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _itemRepository.DeleteAsync(id);
        return NoContent();
    }
}