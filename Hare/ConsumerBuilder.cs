using System.Text.Json.Serialization;
using EasyNetQ;

namespace Merviche.Hare;

public class ConsumerBuilder<T>
{
    /// <summary>
    /// If null, your queue will be bound to the default exchange.
    /// </summary>
    public string Exchange { get; set; } = string.Empty;

    public required string Queue { get; set; }
    public string RoutingKey { get; set; } = string.Empty;

    /// <summary>
    /// Will be assigned a <see cref="Guid"/> if null.
    /// </summary>
    public string? ConsumerTag { get; set; }

    /// <summary>
    /// Exceptions here are caught, logged, and nacked without requeue.
    /// </summary>
    public required OnReceived<T> Handler { get; set; }

    /// <summary>
    /// Defaults to <see cref="System.Text.Json.JsonSerializer"/>
    /// Exceptions here are caught, logged, and nacked without requeue.
    /// </summary>
    public OnDeserialize<T>? Deserializer { get; set; }

    /// Required for AOT and trimming.
    public JsonSerializerContext? JsonSerializerContext { get; set; }

    public IConsumer Build(IBus bus, IServiceProvider sp)
    {
        return new Consumer<T>(bus.Advanced, this, sp);
    }
}
