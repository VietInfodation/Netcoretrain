using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitmqProducer.Interfaces;
using System.Text;

namespace RabbitmqProducer.Services
{
    public class RabitMQProducer : IRabitMQProducer
    {
        public IActionResult SendProductMessage<dynamic>(dynamic message)
        {
            try 
            {
                //Here we specify the Rabbit MQ Server. we use rabbitmq docker image and use it
                var factory = new ConnectionFactory
                {
                    HostName = "localhost"
                };
                //Create the RabbitMQ connection using connection factory details as i mentioned above
                var connection = factory.CreateConnection();
               
                //Here we create channel with session and model
                using
                var channel = connection.CreateModel();
                //declare the queue after mentioning name and a few property related to that
                channel.QueueDeclare("export.request", exclusive: false);
                //Serialize the message
                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);
                //put the data on to the product queue
                channel.BasicPublish(exchange: "", routingKey: "export.request", body: body);
                return new OkObjectResult("Sent");
            } 
            catch(Exception e) { return new BadRequestObjectResult(e); }
            
        }
    }
}
