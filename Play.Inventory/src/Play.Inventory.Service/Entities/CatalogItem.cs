using Play.Common.Entities;

namespace Play.Inventory.Service.Entities
{
    public class CatalogItem : IEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}