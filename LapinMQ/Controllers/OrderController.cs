using System.Threading.Tasks;
using LapinMQ.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace LapinMQ.Controllers
{
    [ApiController]
    [Route("/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IPublishEndpoint _endpoint;

        public OrderController(IPublishEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        [HttpPost("redelivery")]
        public async Task<IActionResult> Create(OrderRedelivery order)
        {
            await _endpoint.Publish(order);

            return Accepted(order);
        }
        
    }
}