namespace Trading.Exceptions
{
    [Serializable]
    internal class UnknownItemException : Exception
    {
        public Guid ItemId { get; }

        public UnknownItemException(Guid ItemId) : base($"Unknown item {ItemId} ")
        {
            this.ItemId = ItemId;
        }
    }
}