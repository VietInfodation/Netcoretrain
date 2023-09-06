using Microsoft.AspNetCore.Mvc;

namespace RabbitmqProducer.Interfaces
{
    public interface IRabitMQProducer
    {
        IActionResult SendProductMessage<dynamic>(dynamic message);
    }
}
