using System.Threading.Tasks;
using LapinMQ.Contracts;
using LapinMQ.Exceptions;
using MassTransit;

namespace LapinMQ.Consumers
{
    public class OrderRedeliveryConsumer : IConsumer<OrderRedelivery>
    {
        public Task Consume(ConsumeContext<OrderRedelivery> context)
        {
            var message = $@"RedeliveryCount: {context.GetRedeliveryCount()}
                             Retry Attempt:{context.GetRetryAttempt()}
                             Retry Count:{context.GetRetryCount()}";
            
            throw new RedeliveryException(message);
        }
    }
}