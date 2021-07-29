using System;
using Automatonymous;
using LapinMQ.Contracts;
using MassTransit;
using MassTransit.Saga;

namespace LapinMQ.StateMachines
{
    public class OrderState : SagaStateMachineInstance, ISagaVersion
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        
        public int Version { get; set; }
        public int OrderNumber { get; set; }
        
        public DateTime CreatedOn { get; set; }
        public DateTime? RescheduledOn { get; set; }
        public DateTime? FinalizedOn { get; set; }
        public Guid? TokenId { get; set; }
    }

    public class GracePeriodTimeout
    {
        public Guid CorrelationId { get; set; }
    }

    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);
            
            Event(() => SubmitOrder, e => e
                .CorrelateBy((instance, context) => instance.OrderNumber == context.Message.Number)
                .SelectId(_ => NewId.NextGuid()));
            
            Schedule(() => GracePeriodTimeout, instance => instance.TokenId, s =>
            {
                s.Delay = TimeSpan.FromSeconds(10);
                s.Received = e => e.CorrelateById(x => x.Message.CorrelationId);
            });
            
            Initially(
                When(SubmitOrder)
                    .Then(ctx =>
                    {
                        ctx.Instance.CreatedOn = DateTime.UtcNow;
                        ctx.Instance.OrderNumber = ctx.Data.Number;
                    })
                    .Schedule(GracePeriodTimeout, GracePeriodTimeoutFactory)
                    .TransitionTo(Submitted));

            During(Submitted,
                When(SubmitOrder)
                    .Then(x =>
                    {
                        x.Instance.RescheduledOn = DateTime.UtcNow;
                    })
                    .Schedule(GracePeriodTimeout, GracePeriodTimeoutFactory)
                ,
                When(GracePeriodTimeout.Received)
                    .Then(x =>
                    {
                        x.Instance.FinalizedOn = DateTime.UtcNow;
                    })
                    .Finalize()
                );

            SetCompletedWhenFinalized();
        }


        public State Submitted { get; private set; }
        public Event<SubmitOrder> SubmitOrder { get; private set; }
        public Schedule<OrderState, GracePeriodTimeout> GracePeriodTimeout { get; private set; }
        
        private GracePeriodTimeout GracePeriodTimeoutFactory(ConsumeEventContext<OrderState, SubmitOrder> context)
        {
            return new()
            {
                CorrelationId = context.Instance.CorrelationId,
            };
        }
    }
}