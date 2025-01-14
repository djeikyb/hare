using System.Text.Json.Serialization;

namespace Hare.Demo;

[JsonSerializable(typeof(SomeMessagePayload))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
public partial class AppJsonContext : JsonSerializerContext
{
}

/// {"Hello":"🌏"}
public class SomeMessagePayload
{
    public required string Hello { get; set; }
}

public class SomeService
{
    private readonly ILogger<SomeService> _logger;

    public SomeService(FakeDbContext db, ILogger<SomeService> logger)
    {
        _logger = logger;
    }

    public Task Handle(SomeMessagePayload payload)
    {
        _logger.LogInformation($"Hello {payload.Hello}");
        return Task.CompletedTask;
    }
}

public class FakeDbContext
{
}
