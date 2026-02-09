using MassTransit;
using Payroll.Application.Interfaces;

namespace Payroll.Infrastructure.Messaging;

/// <summary>
/// Implementacija IEventPublisher koristeći MassTransit + RabbitMQ.
/// </summary>
public class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        await _publishEndpoint.Publish(message, cancellationToken);
    }
}
