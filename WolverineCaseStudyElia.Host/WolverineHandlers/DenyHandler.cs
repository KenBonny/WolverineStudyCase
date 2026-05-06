using Wolverine;
using WolverineCaseStudyElia.Contracts;
using WolverineCaseStudyElia.Host.Sagas;

namespace WolverineCaseStudyElia.Host.WolverineHandlers;

public class DenyHandler
{
    public static TimedApprovalSaga? Load(SagaDenied command, TimedApprovalSagaDbContext db)
    {
        return db.TimedApprovalSagas.Find(command.Id);
    }
    
    public static RequirementResult Validate(TimedApprovalSaga? saga, SagaDenied command)
    {
        if (saga is null)
            return new(HandlerContinuation.Stop, [$"Unknown saga with id {command.Id}"]);

        if (saga.Status != TimedApprovalSagaStatus.Started)
            return new(
                HandlerContinuation.Stop,
                [$"Saga with id {command.Id} is in status {saga.Status} and cannot be approved"]);
        
        return RequirementResult.AllGood();
    }
    
    public static StopTimedApprovalSaga Handle(SagaDenied command, TimedApprovalSaga saga, ILogger<ApprovedHandler> logger)
    {
        logger.LogInformation(
            "Handling DenyTimedApprovalSaga command for saga {sagaId} in {SagaStatus}",
            saga.Id,
            saga.Status);
        // LogHandlingApprovetimedapprovalsagaCommandForSagaSagaid(logger, saga.Id, saga.Status);
        return new StopTimedApprovalSaga(saga.Id);
    }

    // [LoggerMessage(LogLevel.Information, "Handling DenyTimedApprovalSaga command for saga {sagaId} in {SagaStatus}")]
    // static partial void LogHandlingApprovetimedapprovalsagaCommandForSagaSagaid(
    //     ILogger<ApproveHandler> logger,
    //     Guid sagaId,
    //     TimedApprovalSagaStatus sagaStatus);
}