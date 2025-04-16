namespace Play.Inventory.Service
{
    // you can use record to replace class if you want to:
    // - Define a data model that depends on value equality
    // - Define a type for which objects are immutable
    public record GrantItemDto(Guid UserId, Guid CatalogItemId, int Quantity);
    public record InventoryItemDto(Guid CatalogItemId, string Name, string Description, int Quantity, DateTimeOffset AcquiredDate);
    public record CatalogItemDto(Guid Id, string Name, string Description);
}