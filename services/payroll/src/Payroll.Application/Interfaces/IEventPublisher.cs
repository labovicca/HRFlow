namespace Payroll.Application.Interfaces;

/// <summary>
/// Interface za objavljivanje event-a na message bus (RabbitMQ).
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
}
