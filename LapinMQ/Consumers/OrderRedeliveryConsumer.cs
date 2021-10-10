using System.Threading.Tasks;
using LapinMQ.Contracts;
using LapinMQ.Exceptions;
using MassTransit;

namespace LapinMQ.Consumers
{
    public class OrderRedeliveryConsumer : IConsumer<RedeliverOrder>
    {
        public Task Consume(ConsumeContext<RedeliverOrder> context)
        {
            var message = $@"RedeliveryCount: {context.GetRedeliveryCount()}
                             Retry Attempt:{context.GetRetryAttempt()}
                             Retry Count:{context.GetRetryCount()}";
            
            throw new RedeliveryException(message);
        }
    }
}