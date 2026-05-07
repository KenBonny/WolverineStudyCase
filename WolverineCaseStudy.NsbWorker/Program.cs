using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, services, config) =>
        config.ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services))
    .UseNServiceBus(context =>
    {
        var endpointConfiguration = new EndpointConfiguration("WolverineCaseStudy");
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();

        var connectionString = context.Configuration.GetConnectionString("RabbitMQ")
            ?? throw new InvalidOperationException("RabbitMQ connection string is not configured.");

        var transport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), connectionString);
        endpointConfiguration.UseTransport(transport);

        return endpointConfiguration;
    })
    .Build();

await host.RunAsync();
