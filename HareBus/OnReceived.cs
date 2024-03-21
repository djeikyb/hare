using EasyNetQ;
using EasyNetQ.Consumer;
using Microsoft.Extensions.DependencyInjection;

namespace HareBus;

public delegate Task<AckStrategy> OnReceived<in T>(
    T payload,
    IServiceScope scope,
    MessageProperties properties,
    MessageReceivedInfo info,
    CancellationToken ct
);
