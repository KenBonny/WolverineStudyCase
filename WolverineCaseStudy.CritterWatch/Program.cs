using CritterWatch.Services.Hosting;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// critterwatch: https://critterwatch.jasperfx.net
builder.AddCritterWatch(
    builder.Configuration.GetConnectionString("CritterWatch") ??
    throw new InvalidOperationException("CritterWatch connection string is not configured."),
    options =>
    {
        var rabbitUri = new Uri(
            builder.Configuration.GetConnectionString("RabbitMQ") ??
            throw new InvalidOperationException("RabbitMQ connection string is not configured."));

        options.UseRabbitMq(rabbitUri).AutoProvision();
        options.ListenToRabbitQueue("critterwatch").Sequential();
    });

var app = builder.Build();

app.UseCritterWatch();

app.Run();