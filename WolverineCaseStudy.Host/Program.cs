using JasperFx;
using JasperFx.Resources;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Wolverine;
using Wolverine.CritterWatch;
using Wolverine.EntityFrameworkCore;
using Wolverine.Http;
using Wolverine.Persistence;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;
using Wolverine.Runtime.Heartbeat;
using WolverineCaseStudy.Contracts;
using WolverineCaseStudy.Host.Sagas;

var builder = WebApplication.CreateBuilder(args);
var isTesting = builder.Environment.IsEnvironment("Testing");

builder.Host.UseSerilog((context, services, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .ReadFrom.Services(services));

var postgres = builder.Configuration.GetConnectionString("TimedApprovalSagaDb") ??
    throw new InvalidOperationException("Connection string for TimedApprovalSagaDb is not configured.");

// Set up Wolverine
if (!isTesting)
    builder.Host.UseWolverine(options =>
    {
        options.EnableHeartbeats();
        options.PublishMessage<WolverineHeartbeat>().ToRabbitQueue("critterwatch");

        options.UseEntityFrameworkCoreTransactions(TransactionMiddlewareMode.Lightweight);

        var rabbitUri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ")
            ?? throw new InvalidOperationException("RabbitMQ connection string is not configured."));

        options.UseRabbitMq(rabbitUri)
            .AutoProvision()
            .UseConventionalRouting();
        
        var nsbBroker = new BrokerName("nsb");
        options
            .AddNamedRabbitMqBroker(
                nsbBroker,
                factory =>
                {
                    var nsbRabbitUri = new Uri(
                        builder.Configuration.GetConnectionString("NsbRabbitMQ") ??
                        throw new InvalidOperationException(
                            "RabbitMQ connection string for NServiceBus is not configured."));
                    factory.Uri = nsbRabbitUri;
                })
            .AutoProvision()
            .BindExchange("WolverineCaseStudy.Contracts:WrittenToConsole")
            .ToQueue("WolverineCaseStudy.NsbInterop");

        options.AddCritterWatchMonitoring(
            critterWatchUri: new Uri("rabbitmq://queue/critterwatch"),
            systemControlUri: new Uri("rabbitmq://queue/trip-service-control"));

        options.PublishMessage<WriteToConsole>()
            .ToRabbitQueue("WolverineCaseStudy")
            .UseNServiceBusInterop();

        options.ListenToRabbitQueue("WolverineCaseStudy.NsbInterop")
            .UseNServiceBusInterop();
        
        options.Policies.DisableConventionalLocalRouting();
        options.Policies.AutoApplyTransactions();
        options.Policies.UseDurableInboxOnAllListeners();
        options.Policies.UseDurableOutboxOnAllSendingEndpoints();
        options.Policies.UseDurableLocalQueues();
        options.Policies.AlwaysMakeScheduledMessagesDurable();
        options.PersistMessagesWithPostgresql(postgres);

        options.Services.AddResourceSetupOnStartup();
    });


builder.Services.AddWolverineHttp();
builder.Services.Configure<TimedApprovalSagaOptions>(builder.Configuration.GetSection("TimedApprovalSaga"));
builder.Services.AddDbContext<TimedApprovalSagaDbContext>(options =>
    options.UseNpgsql(postgres));

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

if (!isTesting)
{
    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();

    builder.Services.AddAuthorization(options =>
    {
        // By default, all incoming requests will be authorized according to the default policy.
        options.FallbackPolicy = options.DefaultPolicy;
    });
}
else
{
    builder.Services.AddAuthorization();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapWolverineEndpoints(options =>
{
    if (!isTesting)
    {
        options.RequireAuthorizeOnAll();
    }
});

// in memory workaround
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TimedApprovalSagaDbContext>();
    await db.Database.MigrateAsync();
}

await app.RunJasperFxCommands(args);