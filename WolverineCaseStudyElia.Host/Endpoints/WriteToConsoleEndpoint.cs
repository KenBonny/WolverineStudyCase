using Wolverine.Http;
using WolverineCaseStudyElia.Host.Handlers;

namespace WolverineCaseStudyElia.Host.Endpoints;

public static class WriteToConsoleEndpoint
{
    [WolverinePost("/console")]
    public static async Task<IResult> Post(WriteToConsole message, IMessageSession session)
    {
        await session.Send(message);
        return Results.Accepted();
    }
}
