using MassTransit;
using Play.Catalog.Contracts;
using Play.Common.Repositories;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemUpdatedConsumer : IConsumer<CatalogItemUpdated>
    {
        private readonly IRepository<CatalogItem> _catalogRepository;
        public CatalogItemUpdatedConsumer(IRepository<CatalogItem> catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }
        public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
        {
            var message = context.Message;
            var item = await _catalogRepository.GetByIdAsync(message.ItemId);
            if (item == null)
            {
                CatalogItem newItem = new CatalogItem
                {
                    Id = message.ItemId,
                    Name = message.Name,
                    Description = message.Description
                };
                await _catalogRepository.CreateAsync(newItem);
            }
            else
            {
                item.Name = message.Name;
                item.Description = message.Description;

                await _catalogRepository.UpdateAsync(item);
            }
        }
    }
}