using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;
using WolverineCaseStudy.Contracts;

namespace WolverineCaseStudy.Host.Sagas;


/// <summary>
/// Endpoint for handling saga actions for the timed approval saga.
/// This is a single endpoint that can handle all actions for the saga,
/// including starting, approving, denying, and stopping the saga.
/// The endpoint will validate the incoming request and post the appropriate command
/// to the bus based on the action specified in the query parameters.
/// </summary>
public static class TimedApprovalSagaEndpoint
{
    public static ProblemDetails Validate(SagaAction action, Guid? sagaId)
    {
        var sagaIdPresent = sagaId is not null;
        if (action == SagaAction.Start || sagaIdPresent)
            return WolverineContinue.NoProblems;
        
        return new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid request",
            Detail = "The sagaId query parameter is required for Approve, Deny, and Stop actions."
        };

    }

    [Tags("TimedApprovalSaga")]
    [EndpointSummary("SagaAction")]
    [EndpointDescription("Handles the incoming request for the saga action endpoint.")]
    [WolverinePost("/saga", OperationId = "Sagaaaa")]
    public static (IResult accpetedResult, OutgoingMessages) Handle(SagaAction action, Guid? sagaId)
    {
        var id = sagaId ?? Guid.NewGuid();
        object command = action switch
        {
            SagaAction.Start => new StartTimedApprovalSaga(id),
            SagaAction.Approve => new ApproveTimedApprovalSaga(id),
            SagaAction.Deny => new DenyTimedApprovalSaga(id),
            SagaAction.Stop => new StopTimedApprovalSaga(id),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Unsupported saga action")
        };

        var accpetedResult = Results.Accepted(
            $"/saga?action={action}&sagaId={id}",
            new
            {
                SagaId = id,
                Action = action.ToString()
            });
        return (accpetedResult, new OutgoingMessages([command]));
    }
}