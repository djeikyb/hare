using EasyNetQ;

namespace Merviche.Hare;

public delegate Task<T?> OnDeserialize<T>(
    ReadOnlyMemory<byte> msg,
    MessageProperties properties,
    MessageReceivedInfo info,
    CancellationToken ct
);
