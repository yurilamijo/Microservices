namespace Contracts
{
    public class IdentityContracts
    {
        public record DebitPoints(Guid UserId, decimal Points, Guid CorrelationId);

        public record PointsDebited(Guid CorrelationId);
    }
}
