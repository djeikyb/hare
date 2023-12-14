// using System.Text.Json;
// using System.Text.Json.Serialization;
// using RabbitMQ.Client;
// using RabbitMQ.Client.Events;
//
// namespace hare;
//
// public interface IRabbit
// {
// }
//
// class JsonPublisherBuilder
// {
//     public required string Exchange { get; set; }
//     public required string RoutingKey { get; set; }
//     public required JsonSerializerContext JsonSerializerContext { get; set; }
//
//     public async Task<JsonTopicPublisher<T>> Build<T>(IChannel channel, CancellationToken ct = default)
//     {
//         await channel.ConfirmSelectAsync();
//         await channel.ExchangeDeclareAsync(exchange: Exchange, type: "topic", durable: true, autoDelete: false);
//         return new JsonTopicPublisher<T>(channel, Exchange, RoutingKey, JsonSerializerContext);
//     }
// }
//
// class JsonTopicPublisher<T>
// {
//     public IChannel Channel { get; set; }
//     public string Exchange { get; set; }
//     public string RoutingKey { get; set; }
//     public JsonSerializerContext JsonSerializerContext { get; set; }
//
//     public JsonTopicPublisher(
//         IChannel channel,
//         string exchange,
//         string routingKey,
//         JsonSerializerContext jsonSerializerContext
//     )
//     {
//         Channel = channel;
//         Exchange = exchange;
//         RoutingKey = routingKey;
//         JsonSerializerContext = jsonSerializerContext;
//     }
//
//     public async Task Publish(T payload, CancellationToken ct = default)
//     {
//         await Channel.ConfirmSelectAsync();
//         await Channel.ExchangeDeclareAsync(exchange: Exchange, type: "topic", durable: true, autoDelete: false);
//         var json = JsonSerializer.SerializeToUtf8Bytes(
//             value: payload,
//             inputType: typeof(T),
//             context: JsonSerializerContext
//         );
//         await Channel.BasicPublishAsync(exchange: Exchange, routingKey: RoutingKey, body: json, mandatory: false);
//     }
// }
//
// class JsonTopicConsumerBuilder<T>
// {
//     public required string Exchange { get; set; }
//     public required string Queue { get; set; }
//     public required string RoutingKey { get; set; }
//     public required JsonSerializerContext JsonSerializerContext { get; set; }
//     public required IConsumer<T> Consumer { get; set; }
//
//     public async Task<JsonTopicConsumer<T>> Build(IChannel channel, CancellationToken ct = default)
//     {
//         await channel.ExchangeDeclareAsync(exchange: Exchange, type: "topic", durable: true, autoDelete: false);
//         await channel.QueueDeclareAsync(queue: Queue, exclusive: false, durable: true, autoDelete: false);
//         await channel.QueueBindAsync(Queue, Exchange, RoutingKey);
//         return new JsonTopicConsumer<T>(Consumer, JsonSerializerContext);
//     }
// }
//
// interface IConsumer<T>
// {
//     public Task Consume(T body, CancellationToken ct = default);
// }
//
// interface IPublisher<T>
// {
//     public Task Publish(T body, CancellationToken ct = default);
// }
//
// class JsonTopicConsumer<T>
// {
//     private readonly IConsumer<T> _consumer;
//     private JsonSerializerContext _jsonSerializerContext;
//
//     public JsonTopicConsumer(IConsumer<T> consumer, JsonSerializerContext jsonSerializerContext)
//     {
//         _consumer = consumer;
//         _jsonSerializerContext = jsonSerializerContext;
//         channelre
//     }
//
//     public void Start(CancellationToken ct = default)
//     {
//         var consumer = new AsyncEventingBasicConsumer(channel:);
//         await chan.BasicConsumeAsync(consumer, Queue, true);
//     }
//
//     public T? HandleReceived(BasicDeliverEventArgs ea)
//     {
//         return (T?)JsonSerializer.Deserialize(ea.Body.Span, typeof(T), _jsonSerializerContext);
//     }
// }
//
// class MyTopicConsumer
// {
//     public required string Exchange { get; set; }
//     public required string Queue { get; set; }
//     public required string RoutingKey { get; set; }
//     public required JsonSerializerContext JsonSerializerContext { get; set; }
//
//
//     public async Task<AsyncEventingBasicConsumer> Build(IChannel chan)
//     {
//         await chan.ExchangeDeclareAsync(exchange: Exchange, type: "topic", durable: true, autoDelete: false);
//         await chan.QueueDeclareAsync(queue: Queue, exclusive: false, durable: true, autoDelete: false);
//         await chan.QueueBindAsync(Queue, Exchange, RoutingKey);
//         var consumer = new AsyncEventingBasicConsumer(chan);
//         await chan.BasicConsumeAsync(consumer, Queue, true);
//     }
// }
//
// public class Class1
// {
//     public async Task Foo()
//     {
//         var factory = new ConnectionFactory();
//         factory.DispatchConsumersAsync = true;
//
//         var conn = await factory.CreateConnectionAsync();
//         conn.ConnectionShutdown += (o, ea) =>
//         {
//             if (ea.Initiator == ShutdownInitiator.Peer)
//             {
//             }
//         };
//
//         var chan = await conn.CreateChannelAsync();
//         chan.ChannelShutdown += (o, ea) =>
//         {
//             if (ea.Initiator == ShutdownInitiator.Peer)
//             {
//             }
//         };
//
//         var consumer = new AsyncEventingBasicConsumer(chan);
//         consumer.Shutdown += (o, ea) =>
//         {
//             if (ea.Initiator == ShutdownInitiator.Peer)
//             {
//             }
//
//             return Task.CompletedTask;
//         };
//
//         consumer.Received += (o, ea) => { };
//
//         await chan.BasicConsumeAsync(consumer, "some queue", autoAck: true, noLocal: false);
//     }
// }
