using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Wolverine;
using Wolverine.Http;
using Wolverine.RabbitMQ;
using WolverineCaseStudyElia.Contracts;
using WolverineCaseStudyElia.Host.Sagas;

var builder = WebApplication.CreateBuilder(args);
var isTesting = builder.Environment.IsEnvironment("Testing");

builder.Host.UseSerilog((context, services, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .ReadFrom.Services(services));

// Set up Wolverine
if (!isTesting)
    builder.Host.UseWolverine(options =>
    {
        var rabbitUri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ")
            ?? throw new InvalidOperationException("RabbitMQ connection string is not configured."));

        options.UseRabbitMq(rabbitUri)
            .AutoProvision()
            .BindExchange("WolverineCaseStudyElia.Contracts:WrittenToConsole")
            .ToQueue("WolverineCaseStudyElia.NsbInterop");

        options.PublishMessage<WriteToConsole>()
            .ToRabbitQueue("WolverineCaseStudyElia")
            .UseNServiceBusInterop();

        options.ListenToRabbitQueue("WolverineCaseStudyElia.NsbInterop")
            .UseNServiceBusInterop();

        options.Discovery.IncludeType<TimedApprovalSaga>();

        options.Policies.AutoApplyTransactions();
    });

builder.Services.AddWolverineHttp();
builder.Services.Configure<TimedApprovalSagaOptions>(builder.Configuration.GetSection("TimedApprovalSaga"));
builder.Services.AddDbContext<TimedApprovalSagaDbContext>(options =>
    options.UseInMemoryDatabase("timed-approval-sagas"));

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

app.Run();

public partial class Program;
