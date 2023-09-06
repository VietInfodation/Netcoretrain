namespace Coursesvc.Interfaces
{
    public interface IRabbitMQConsumer
    {
        Task ReadMessages();
    }
}
