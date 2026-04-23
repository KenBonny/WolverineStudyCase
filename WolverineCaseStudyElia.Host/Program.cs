using Microsoft.AspNetCore.Authentication.Negotiate;
using Scalar.AspNetCore;
using Serilog;
using Wolverine;
using Wolverine.Http;
using Wolverine.RabbitMQ;
using WolverineCaseStudyElia.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .ReadFrom.Services(services));

// Set up Wolverine
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

    options.Policies.AutoApplyTransactions();
});
builder.Services.AddWolverineHttp();

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
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapWolverineEndpoints(options =>
{
    options.RequireAuthorizeOnAll();
});

app.Run();