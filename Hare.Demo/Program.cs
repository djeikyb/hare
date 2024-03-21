using EasyNetQ;
using Hare.Demo;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(
    RabbitHutch.CreateBus(
        "host=localhost;port=5672;virtualHost=/;username=admin;password=admin",

        // We're bypassing most all of the EasyNetQ registered services.
        // But this one is required. Without it, EasyNetQ throws on app
        // start unless you have newtonsoft's json package installed.
        register => register.EnableSystemTextJson()
    )
);
builder.Services.AddTransient<SomeService>();
builder.Services.AddScoped<FakeDbContext>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<WorkerConsumerCustomDeserialize>();

var host = builder.Build();
host.Run();
