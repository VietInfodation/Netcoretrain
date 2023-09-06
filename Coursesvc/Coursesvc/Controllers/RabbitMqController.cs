using Microsoft.AspNetCore.Mvc;

namespace Coursesvc.Controllers
{
    [Route("rabbitmq")]
    public class RabbitMqController : Controller
    {
        /*private readonly IRabbitMQConsumer _consuming;
        public RabbitMqController(IRabbitMQConsumer consuming) 
        {
            _consuming = consuming;
        }
        [HttpGet]
        public async Task<IActionResult> GetMessage()
        {
            await _consuming.ReadMessages();
            return Ok();
        }*/
    }
}
