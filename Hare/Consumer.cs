using System.Text;
using System.Text.Json.Serialization;
using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.Topology;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Merviche.Hare;

public class Consumer<T> : IConsumer
{
    private readonly IAdvancedBus _bus;
    private readonly IServiceProvider _sp;
    private readonly ILogger<Consumer<T>> _logger;
    private IDisposable? _disposable;
    private readonly string _exchange;
    private readonly string _queue;
    private readonly string _routingKey;
    private readonly string? _consumerTag;
    private readonly OnReceived<T> _handler;
    private readonly OnDeserialize<T>? _deserializer;
    private readonly JsonSerializerContext? _jsonSerializerContext;

    public Consumer(IAdvancedBus bus, ConsumerBuilder<T> builder, IServiceProvider sp)
    {
        _bus = bus;
        _sp = sp;
        _logger = sp.GetRequiredService<ILogger<Consumer<T>>>();

        // copy so if builder is safe to mutate
        _exchange = builder.Exchange;
        _queue = builder.Queue;
        _routingKey = builder.RoutingKey;
        _consumerTag = builder.ConsumerTag;
        _handler = builder.Handler;
        _deserializer = builder.Deserializer;
        _jsonSerializerContext = builder.JsonSerializerContext;
    }

    public async Task Start(CancellationToken ct)
    {
        var x = await _bus.ExchangeDeclareAsync(_exchange, ExchangeType.Topic, cancellationToken: ct);
        var q = await _bus.QueueDeclareAsync(_queue, ct);
        await _bus.BindAsync(x, q, _routingKey, cancellationToken: ct);

        var deserializer = _deserializer ??
                           ((msg, _, _, _) =>
                           {
                               if (_jsonSerializerContext is { } jsc)
                               {
                                   var obj = (T?)JsonSerializer.Deserialize(msg.Span, typeof(T), jsc);
                                   return Task.FromResult(obj);
                               }
                               else
                               {
#pragma warning disable IL2026
                                   // IL2026: Using member 'System.Text.Json.JsonSerializer.Deserialize<TValue>(ReadOnlySpan<Byte>, JsonSerializerOptions)'
                                   // which has 'RequiresUnreferencedCodeAttribute' can break functionality when trimming application code.
                                   var obj = JsonSerializer.Deserialize<T>(msg.Span);
#pragma warning restore IL2026
                                   return Task.FromResult(obj);
                               }
                           });

        _disposable = _bus.Consume(
            q,
            (MessageHandler)Mh,
            o => o.WithConsumerTag(_consumerTag ?? Guid.NewGuid().ToString())
        );
        return;

        async Task<AckStrategy> Mh(
            ReadOnlyMemory<byte> body,
            MessageProperties properties,
            MessageReceivedInfo info,
            CancellationToken token
        )
        {
            if (info.Redelivered)
            {
                var safePreview = body.Length < 255
                    ? Encoding.UTF8.GetString(body.Span)
                    : Encoding.UTF8.GetString(body[..255].Span);
                using var _ = _logger.BeginScope(new Dictionary<string, string> { { "RabbitMessage", safePreview } });
                _logger.LogInformation("We've seen this message before and failed to process. Discarding.");
                return AckStrategies.NackWithoutRequeue;
            }

            T? obj;
            try
            {
                obj = await deserializer(body, properties, info, ct);
                if (obj == null)
                {
                    var safePreview = body.Length < 255
                        ? Encoding.UTF8.GetString(body.Span)
                        : Encoding.UTF8.GetString(body[..255].Span);
                    using var _ = _logger.BeginScope(
                        new Dictionary<string, string> { { "RabbitMessage", safePreview } }
                    );
                    _logger.LogError("Failed to deserialize message. Discarding message.");
                    return AckStrategies.NackWithoutRequeue;
                }
            }
            catch (Exception e)
            {
                var safePreview = body.Length < 255
                    ? Encoding.UTF8.GetString(body.Span)
                    : Encoding.UTF8.GetString(body[..255].Span);
                using var _ = _logger.BeginScope(new Dictionary<string, string> { { "RabbitMessage", safePreview } });
                _logger.LogError(e, "Failed to deserialize message. Discarding message.");
                return AckStrategies.NackWithoutRequeue;
            }

            try
            {
                using var scope = _sp.CreateScope();
                return await _handler(obj, scope, properties, info, ct);
            }
            catch (Exception e)
            {
                var safePreview = body.Length < 255
                    ? Encoding.UTF8.GetString(body.Span)
                    : Encoding.UTF8.GetString(body[..255].Span);
                using var _ = _logger.BeginScope(new Dictionary<string, string> { { "RabbitMessage", safePreview } });
                _logger.LogError(e, "Handler failed for {MessageType}. Discarding message.", obj.GetType());
                return AckStrategies.NackWithoutRequeue;
            }
        }
    }

    public Task Stop(CancellationToken ct)
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _disposable?.Dispose();
    }
}
