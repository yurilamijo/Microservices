using Automatonymous;
using Trading.Activities;
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

        public Event<GetPurchaseState> GetPurchaseState { get; }

        public PurchaseStateMachine()
        {
            InstanceState(state => state.CurrentState);
            ConfigureEvents();
            ConfigureInitialState();
            ConfigureAny();
        }

        /// <summary>
        /// Defines the events for MassTransit
        /// </summary>
        private void ConfigureEvents()
        {
            Event(() => PurchaseRequested);
            Event(() => GetPurchaseState);
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
                    .Activity(x => x.OfType<CalculatePurchaseTotalActivity>())
                    .TransitionTo(Accepted)
                    .Catch<Exception>(ex => ex
                        .Then(context =>
                        {
                            context.Instance.ErrorMessage = context.Exception.Message;
                            context.Instance.LastUpdated = DateTimeOffset.UtcNow;
                        })
                        .TransitionTo(Faulted)
                    )
            );
        }

        private void ConfigureAny()
        {
            DuringAny(
                When(GetPurchaseState)
                    .Respond(x => x.Instance)
            );
        }
    }
}
