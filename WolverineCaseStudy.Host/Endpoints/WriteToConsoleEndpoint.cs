using Wolverine;
using Wolverine.Http;
using WolverineCaseStudy.Contracts;

namespace WolverineCaseStudy.Host.Endpoints;

public class WriteToConsoleEndpoint
{
    private readonly IMessageBus _bus;
    private readonly ILogger _logger;

    public WriteToConsoleEndpoint(IMessageBus bus, ILogger<WriteToConsoleEndpoint> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    [WolverinePost("/console")]
    public async Task<IResult> Post(WriteToConsole message, HttpContext httpContext)
    {
        _logger.LogInformation("Received message from: {IdentityName}", httpContext.User.Identity?.Name ?? "Anonymous");
        await _bus.SendAsync(message);
        return Results.Accepted();
    }
}
