using Automatonymous;
using Trading.Contracts;

namespace Trading.StateMachines
{
    public interface IPurchaseStateMachine
    {
        State Accepted { get; }
        State Completed { get; }
        State Faulted { get; }
        State ItemsGranted { get; }
        Event<PurchaseRequested> PurchaseRequested { get; }
    }
}