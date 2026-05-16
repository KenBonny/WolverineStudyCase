using Wolverine;
using Wolverine.Persistence;
using WolverineCaseStudy.Contracts;

namespace WolverineCaseStudy.Host.Sagas;

public partial class DenyHandler
{
    public static RequirementResult Validate(TimedApprovalSaga? saga, SagaDenied command)
    {
        if (saga is null)
            return new(HandlerContinuation.Stop, [$"Unknown saga with id {command.Id}"]);

        if (saga.Status != TimedApprovalSagaStatus.Denied)
            return new(
                HandlerContinuation.Stop,
                [$"Saga with id {command.Id} is in status {saga.Status} and cannot be approved"]);

        return RequirementResult.AllGood();
    }

    public static StopTimedApprovalSaga Handle(
        SagaDenied command,
        [Entity] TimedApprovalSaga saga,
        ILogger<DenyHandler> logger)
    {
        LogHandlingApprovetimedapprovalsagaCommandForSagaSagaid(logger, saga.Id, saga.Status);
        return new StopTimedApprovalSaga(saga.Id);
    }

    [LoggerMessage(LogLevel.Information, "Handling DenyTimedApprovalSaga command for saga {sagaId} in {SagaStatus}")]
    static partial void LogHandlingApprovetimedapprovalsagaCommandForSagaSagaid(
        ILogger<DenyHandler> logger,
        Guid sagaId,
        TimedApprovalSagaStatus sagaStatus);
}