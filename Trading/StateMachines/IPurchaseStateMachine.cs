using Automatonymous;
using Trading.Contracts;
using static Contracts.IdentityContracts;
using static Contracts.InventoryContracts;

namespace Trading.StateMachines
{
    public interface IPurchaseStateMachine
    {
        State Accepted { get; }
        State Completed { get; }
        State Faulted { get; }
        State ItemsGranted { get; }
        Event<PurchaseRequested> PurchaseRequested { get; }
        Event<GetPurchaseState> GetPurchaseState { get; }
        Event<InventoryItemsGranted> InventoryItemsGranted { get; }
        Event<PointsDebited> PointsDebited { get; }
    }
}