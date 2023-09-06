using Coursesvc.Interfaces;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Coursesvc.Services
{
    public class RabbitMQConsumer : IRabbitMQConsumer, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel channel;
        private readonly IFile _fileservice;

        public RabbitMQConsumer(IFile fileservice)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            channel = _connection.CreateModel();
            channel.QueueDeclare("export.request", exclusive: false);
            _fileservice = fileservice;
        }

        /*public IModel GetModel()
        {
            return this.channel;
        }*/

        public async Task ReadMessages()
        {

            var consumer = new AsyncEventingBasicConsumer(channel);
            //consumer.Received += Consumer_Received;
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Process the message (invoke the Consumer API)

                Console.WriteLine($"Product message received: {message}");
                //var process = ProcessMessageAsync(message).Exception;

                /*if (process != null)
                {
                    channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: true);
                    return;
                }*/

                var result = await Task.Run(() => _fileservice.WriteCSV());

                if (result is StatusCodeResult statusCodeResult)
                {
                    var statusCode = statusCodeResult.StatusCode;
                    if (statusCode != 200)
                        channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: true);
                }

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };
            //channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            channel.BasicConsume(queue: "export.request", autoAck: false, consumer: consumer);
            await Task.CompletedTask;
            //Thread.Sleep(Timeout.Infinite);
        }

        /* private async Task ProcessMessageAsync(string message)
         {
             await _fileservice.WriteCSV();
         }*/

        private async void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Product message received: {message}");
                await _fileservice.WriteCSV();
                channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                channel.BasicReject(deliveryTag: e.DeliveryTag, requeue: true);
            }
        }
        public void Dispose()
        {
            if (channel.IsOpen)
                channel.Close();
            if (_connection.IsOpen)
                _connection.Close();
        }
    }
}

/*consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Process the message (invoke the Consumer API)
                await Task.Run(() => _fileservice.WriteEmployeeCSV());
                //var process = ProcessMessageAsync(message).Exception;
                *//*if (process != null) { 
                    channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: true); 
                    return; 
                }*//*
                Console.WriteLine($"Product message received: {message}");

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };*/
