namespace Contracts
{
    public class CatalogContracts
    {
        public record CatalogItemCreated(Guid ItemId, string Name, decimal Price, string Description);

        public record CatalogItemUpdated(Guid ItemId, string Name, decimal Price, string Description);

        public record CatalogItemDeleted(Guid ItemId);
    }
}
