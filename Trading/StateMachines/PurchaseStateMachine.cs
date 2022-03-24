using Automatonymous;
using Trading.Contracts;

namespace Trading.StateMachines
{
    public class PurchaseStateMachine : MassTransitStateMachine<PurchaseState>, IPurchaseStateMachine
    {
        public State Accepted { get; }

        public State ItemsGranted { get; }

        public State Completed { get; }

        public State Faulted { get; }

        public Event<PurchaseRequested> PurchaseRequested { get; }

        public PurchaseStateMachine()
        {
            InstanceState(state => state.CurrentState);
            ConfigureEvents();
            ConfigureInitialState();
        }

        /// <summary>
        /// Defines the events for MassTransit
        /// </summary>
        private void ConfigureEvents()
        {
            Event(() => PurchaseRequested);
        }

        /// <summary>
        /// Configures the initial state of the events
        /// </summary>
        private void ConfigureInitialState()
        {
            Initially(
                When(PurchaseRequested)
                    .Then(context =>
                    {
                        context.Instance.UserId = context.Data.UserId;
                        context.Instance.ItemId = context.Data.ItemId;
                        context.Instance.CorrelationId = context.Data.CorrelationId;
                        context.Instance.Quantity = context.Data.Quantity;
                        context.Instance.Received = DateTimeOffset.UtcNow;
                        context.Instance.LastUpdated = context.Instance.Received;
                    })
                    .TransitionTo(Accepted)
            );
        }
    }
}
