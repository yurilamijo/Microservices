using System.Runtime.Serialization;

namespace Identity.Exceptions
{
    [Serializable]
    internal class InsufficientFundsException : Exception
    {
        public Guid UserId { get; }
        public decimal PointsToDebit { get; }

        public InsufficientFundsException(Guid userId, decimal points) : base($"Not enough points to debit {points} from user {userId}")
        {
            this.UserId = userId;
            this.PointsToDebit = points;
        }
    }
}