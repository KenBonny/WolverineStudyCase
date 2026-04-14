using Microsoft.AspNetCore.Authentication.Negotiate;
using EndpointConfiguration = NServiceBus.EndpointConfiguration;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNServiceBus(context =>
{
    var endpointConfiguration = new EndpointConfiguration("WolverineCaseStudyElia");
    endpointConfiguration.UseSerialization<SystemJsonSerializer>();

    var connectionString = context.Configuration.GetConnectionString("RabbitMQ")
        ?? throw new InvalidOperationException("RabbitMQ connection string is not configured.");

    var transport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), connectionString);
    var routing = endpointConfiguration.UseTransport(transport);
    routing.RouteToEndpoint(typeof(WriteToConsole).Assembly, "WolverineCaseStudyElia");

    endpointConfiguration.EnableInstallers();

    return endpointConfiguration;
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();