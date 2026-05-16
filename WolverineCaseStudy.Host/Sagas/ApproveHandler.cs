using Wolverine;
using WolverineCaseStudy.Contracts;

namespace WolverineCaseStudy.Host.Sagas;

public class ApprovedHandler
{
    public static (TimedApprovalSaga?, DateTimeOffset) Load(SagaApproved command, TimedApprovalSagaDbContext db)
    {
        return (db.TimedApprovalSagas.Find(command.Id), DateTimeOffset.UtcNow);
    }

    public static RequirementResult Validate(TimedApprovalSaga? saga, SagaApproved command, DateTimeOffset now)
    {
        if (saga is null)
            return new(HandlerContinuation.Stop, [$"Unknown saga with id {command.Id}"]);

        if (saga.Status != TimedApprovalSagaStatus.Approved)
            return new(
                HandlerContinuation.Stop,
                [$"Saga with id {command.Id} is in status {saga.Status} and cannot be approved"]);
        
        if (now > saga.EndedAtUtc)
            return new(
                HandlerContinuation.Stop,
                [$"Saga with id {command.Id} has expired and cannot be approved"]);
        
        return RequirementResult.AllGood();
    }
    
    public static StopTimedApprovalSaga Handle(SagaApproved command, TimedApprovalSaga saga, ILogger<ApprovedHandler> logger)
    {
        logger.LogInformation(
            "Handling ApproveTimedApprovalSaga command for saga {sagaId} in {SagaStatus}",
            saga.Id,
            saga.Status);
        return new StopTimedApprovalSaga(saga.Id);
    }
}