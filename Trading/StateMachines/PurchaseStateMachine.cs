using Automatonymous;
using Trading.Activities;
using Trading.Contracts;
using static Contracts.IdentityContracts;
using static Contracts.InventoryContracts;

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

        public Event<InventoryItemsGranted> InventoryItemsGranted { get; }

        public Event<PointsDebited> PointsDebited { get; }


        public PurchaseStateMachine()
        {
            InstanceState(state => state.CurrentState);
            ConfigureEvents();
            ConfigureInitialState();
            ConfigureAny();
            ConfigureAccepted();
            ConfigureCompleted();
        }

        /// <summary>
        /// Defines the events for MassTransit
        /// </summary>
        private void ConfigureEvents()
        {
            Event(() => PurchaseRequested);
            Event(() => GetPurchaseState);
            Event(() => InventoryItemsGranted);
            Event(() => PointsDebited);
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
                    .Send(context => new GrandItems(
                        context.Instance.UserId,
                        context.Instance.ItemId,
                        context.Instance.Quantity,
                        context.Instance.CorrelationId))
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

        /// <summary>
        /// Configures the accepted state
        /// </summary>
        private void ConfigureAccepted()
        {
            During(Accepted,
                When(InventoryItemsGranted)
                    .Then(context =>
                    {
                        context.Instance.LastUpdated = DateTimeOffset.UtcNow;
                    })
                    .Send(context => new DebitPoints(
                        context.Instance.UserId,
                        context.Instance.PurchaseTotal,
                        context.Instance.CorrelationId
                     ))
                    .TransitionTo(ItemsGranted));
        }

        /// <summary>
        /// Configures the completed state
        /// </summary>
        private void ConfigureCompleted()
        {
            During(ItemsGranted,
                When(PointsDebited)
                    .Then(context => {
                        context.Instance.LastUpdated = DateTimeOffset.UtcNow;
                    })
                    .TransitionTo(Completed)
            ); ;
        }

        /// <summary>
        /// Configures the response on a purchase
        /// </summary>
        private void ConfigureAny()
        {
            DuringAny(
                When(GetPurchaseState)
                    .Respond(x => x.Instance)
            );
        }
    }
}
