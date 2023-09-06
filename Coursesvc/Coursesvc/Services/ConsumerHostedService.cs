using Coursesvc.Interfaces;

namespace Coursesvc.Services
{
    public class ConsumerHostedService : BackgroundService
    {
        private readonly IRabbitMQConsumer _consumerService;

        public ConsumerHostedService(IRabbitMQConsumer consumerService)
        {
            _consumerService = consumerService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumerService.ReadMessages();
        }
    }
}
