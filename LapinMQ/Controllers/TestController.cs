using System.Threading.Tasks;
using LapinMQ.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace LapinMQ.Controllers
{
    [ApiController]
    [Route("/test")]
    public class TestController : ControllerBase
    {
        private readonly IPublishEndpoint _endpoint;

        public TestController(IPublishEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        [HttpPost("saga")]
        public async Task<IActionResult> Saga(SubmitOrder order)
        {
            await _endpoint.Publish(order);

            return Ok();
        }
        
        [HttpPost("redeliver")]
        public async Task<IActionResult> Redelivery(RedeliverOrder message)
        {
            await _endpoint.Publish(message);

            return Ok();
        }
        
        
        [HttpPost("ignore-exception")]
        public async Task<IActionResult> IgnoreException(ThrowDontCare message)
        {
            await _endpoint.Publish(message);

            return Ok();
        }
        
    }
}