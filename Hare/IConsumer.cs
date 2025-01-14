namespace Merviche.Hare;

public interface IConsumer : IDisposable
{
    Task Start(CancellationToken ct);
    Task Stop(CancellationToken ct);
}