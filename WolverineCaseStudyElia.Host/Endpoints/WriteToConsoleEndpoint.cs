using Wolverine.Http;
using WolverineCaseStudyElia.Contracts;

namespace WolverineCaseStudyElia.Host.Endpoints;

public class WriteToConsoleEndpoint
{
    private readonly IMessageSession _session;
    private readonly ILogger _logger;

    public WriteToConsoleEndpoint(IMessageSession session, ILogger<WriteToConsoleEndpoint> logger)
    {
        _session = session;
        _logger = logger;
    }
    
    [WolverinePost("/console")]
    public async Task<IResult> Post(WriteToConsole message, HttpContext httpContext)
    {
        _logger.LogInformation("Received message from: {IdentityName}", httpContext.User.Identity?.Name ?? "Anonymous");
        await _session.Send(message);
        return Results.Accepted();
    }
}
