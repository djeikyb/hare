using EasyNetQ;
using EasyNetQ.Consumer;

namespace HareBus.Demo;

public class Worker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IBus _bus;
    private readonly ILogger<Worker> _logger;
    private IConsumer? _consumer;

    public Worker(IServiceProvider serviceProvider, IBus bus, ILogger<Worker> logger)
    {
        _serviceProvider = serviceProvider;
        _bus = bus;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        _consumer = new ConsumerBuilder<SomeMessagePayload>
        {
            Exchange = "some_exchange",
            Queue = "some_queue",
            RoutingKey = "rk",
            ConsumerTag = $"SomeApp-1.0.0-{Guid.NewGuid()}",
            JsonSerializerContext = AppJsonContext.Default,

            // Could override default deserializer like:
            // Deserializer = (msg, properties, info, token)
            // By default, exceptions are caught, logged, and nacked without requeue.
            // => Task.FromResult(JsonSerializer.Deserialize<SomeMessagePayload>(msg.Span)),

            Handler = async (
                SomeMessagePayload payload,
                IServiceScope scope,
                MessageProperties _,
                MessageReceivedInfo _,
                CancellationToken _
            ) =>
            {
                // Could try..catch for custom handling.
                // By default, exceptions are caught, logged, and nacked without requeue.
                var service = scope.ServiceProvider.GetRequiredService<SomeService>();
                await service.Handle(payload);
                return AckStrategies.Ack;
            }
        }.Build(_bus, _serviceProvider);
        await _consumer.Start(ct);
        _logger.LogInformation("Worker started.");
    }

    public async Task StopAsync(CancellationToken ct)
    {
        if (_consumer != null) await _consumer.Stop(ct);
        _logger.LogInformation("Worker stopped.");
    }
}
