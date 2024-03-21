using EasyNetQ;
using EasyNetQ.Consumer;
using Microsoft.Extensions.DependencyInjection;

namespace Merviche.Hare;

public delegate Task<AckStrategy> OnReceived<in T>(
    T payload,
    IServiceScope scope,
    MessageProperties properties,
    MessageReceivedInfo info,
    CancellationToken ct
);
