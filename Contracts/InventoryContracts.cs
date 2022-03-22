namespace Contracts
{
    public class InventoryContracts
    {
        public record GrandItems(Guid UserId, Guid CatalogItemId, int Quantity, Guid CorrelationId);

        public record InventoryItemsGranted(Guid CorrelationId);

        public record SubstractItems(Guid UserId, Guid CatalogItemId, int Quantity, Guid CorrelationId);

        public record InventoryItemsSubstracted(Guid CorrelationId);
    }
}
