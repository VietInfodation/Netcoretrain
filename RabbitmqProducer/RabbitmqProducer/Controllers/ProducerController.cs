using Microsoft.AspNetCore.Mvc;
using RabbitmqProducer.Interfaces;

namespace RabbitmqProducer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProducerController : ControllerBase
    {
        private readonly IRabitMQProducer _producer;
        public ProducerController(IRabitMQProducer producer)
        {
            _producer = producer;
        }

        [HttpPost("send-message")]
        public IActionResult SendMessage([FromBody] string message)
        {
            var sendMessage = _producer.SendProductMessage<string>(message);
            return sendMessage;
        }
    }
}
