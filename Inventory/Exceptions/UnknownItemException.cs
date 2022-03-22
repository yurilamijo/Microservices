using System.Runtime.Serialization;

namespace Inventory.Exceptions
{
    [Serializable]
    internal class UnknownItemException : Exception
    {
        private Guid catalogItemId;

        public UnknownItemException(Guid catalogItemId) : base($"Unknown item {catalogItemId} ")
        {
            this.catalogItemId = catalogItemId;
        }

        public Guid ItemId { get; }
    }
}