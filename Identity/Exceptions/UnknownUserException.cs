using System.Runtime.Serialization;

namespace Identity.Exceptions
{
    [Serializable]
    internal class UnknownUserException : Exception
    {
        public Guid UserId { get; }

        public UnknownUserException(Guid userId) : base($"Unknown User {userId}")
        {
            this.UserId = userId;
        }
    }
}