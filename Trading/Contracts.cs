namespace Trading.Contracts
{
    public record PurchaseRequested(Guid UserId, Guid ItemId, Guid CorrelationId, int Quantity);

    public record GetPurchaseState(Guid CorrelationId);
}
