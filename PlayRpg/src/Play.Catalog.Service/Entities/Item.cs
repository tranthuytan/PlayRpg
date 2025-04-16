using Play.Common.Entities;

namespace Play.Catalog.Service.Entities;

public class Item : IEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}