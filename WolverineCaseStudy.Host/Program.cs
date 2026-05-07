using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Http;
using Wolverine.Persistence;
using Wolverine.RabbitMQ;
using WolverineCaseStudy.Contracts;
using WolverineCaseStudy.Host.Sagas;
using WolverineCaseStudy.Contracts;
using WolverineCaseStudy.Host.Endpoints;
using WolverineCaseStudy.Host.Sagas;

var builder = WebApplication.CreateBuilder(args);
var isTesting = builder.Environment.IsEnvironment("Testing");

builder.Host.UseSerilog((context, services, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .ReadFrom.Services(services));

// Set up Wolverine
if (!isTesting)
    builder.Host.UseWolverine(options =>
    {
        options.UseEntityFrameworkCoreTransactions(TransactionMiddlewareMode.Lightweight);

        var rabbitUri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ")
            ?? throw new InvalidOperationException("RabbitMQ connection string is not configured."));

        options.UseRabbitMq(rabbitUri)
            .AutoProvision()
            .UseConventionalRouting()
            .BindExchange("WolverineCaseStudy.Contracts:WrittenToConsole")
            .ToQueue("WolverineCaseStudy.NsbInterop");

        options.PublishMessage<WriteToConsole>()
            .ToRabbitQueue("WolverineCaseStudy")
            .UseNServiceBusInterop();

        options.ListenToRabbitQueue("WolverineCaseStudy.NsbInterop")
            .UseNServiceBusInterop();
        
        options.Policies.DisableConventionalLocalRouting();
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

// in memory workaround
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TimedApprovalSagaDbContext>();
    db.Database.EnsureCreated();
}
app.Run();