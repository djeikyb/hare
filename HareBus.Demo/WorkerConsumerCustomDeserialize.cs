using System.Text.Json;
using System.Text.Json.Nodes;
using EasyNetQ;
using EasyNetQ.Consumer;

namespace HareBus.Demo;

public class WorkerConsumerCustomDeserialize : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IBus _bus;
    private readonly ILogger<WorkerConsumerCustomDeserialize> _logger;
    private IConsumer? _consumer;

    public WorkerConsumerCustomDeserialize(
        IServiceProvider serviceProvider,
        IBus bus,
        ILogger<WorkerConsumerCustomDeserialize> logger
    )
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
            Queue = "some_queue.2",
            RoutingKey = "#",
            ConsumerTag = $"SomeApp-1.0.0-{Guid.NewGuid()}",

            // By default, exceptions are caught, logged, and nacked without requeue.
            Deserializer = Deserialize,

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

    private Task<SomeMessagePayload?> Deserialize(ReadOnlyMemory<byte> msg, MessageProperties _, MessageReceivedInfo __, CancellationToken ___)
    {
        var j = JsonNode.Parse(msg.Span);
        if (j is null) return Task.FromResult((SomeMessagePayload?)null);
        var h = j.AsObject()
            .Where(x => x.Key.Equals("hello", StringComparison.InvariantCultureIgnoreCase))
            .Where(x => x.Value != null && x.Value.GetValueKind() == JsonValueKind.String)
            .Select(x => x.Value!.GetValue<string>())
            .FirstOrDefault();

        if (h == null)
        {
            throw new JsonException($"Missing required property {nameof(SomeMessagePayload)}.{nameof(SomeMessagePayload.Hello)}.");
        }

        var payload = new SomeMessagePayload { Hello = h };
        return Task.FromResult<SomeMessagePayload?>(payload);
    }

    public async Task StopAsync(CancellationToken ct)
    {
        if (_consumer != null) await _consumer.Stop(ct);
        _logger.LogInformation("Worker stopped.");
    }
}
