using Wolverine;
using WolverineCaseStudyElia.Contracts;

namespace WolverineCaseStudyElia.Host.Sagas;

public class TimedApprovalSaga : Wolverine.Saga
{
    public Guid Id { get; init; }
    public TimedApprovalSagaStatus Status { get; private set; }
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset? EndedAtUtc { get; private set; }

    public static (TimedApprovalSaga saga, ConsoleWriteSideEffect, OutgoingMessages) Start(
        StartTimedApprovalSaga command,
        TimedApprovalSagaOptions options)
    {
        var saga = new TimedApprovalSaga
        {
            Id = command.Id,
            StartedAtUtc = DateTimeOffset.UtcNow,
            Status = TimedApprovalSagaStatus.Started
        };

        var messages = new OutgoingMessages([new TimedApprovalSagaTimedOut(command.Id).DelayedFor(options.Timeout)]);

        return (saga, new ConsoleWriteSideEffect($"Saga {command.Id} started"), messages);
    }

    public ConsoleWriteSideEffect Handle(ApproveTimedApprovalSaga _)
    {
        if (Status != TimedApprovalSagaStatus.Started)
            return null;

        Status = TimedApprovalSagaStatus.Approved;
        return new ConsoleWriteSideEffect($"Saga {Id} was approved");
    }
    
    public ConsoleWriteSideEffect Handle(DenyTimedApprovalSaga _)
    {
        if (Status != TimedApprovalSagaStatus.Started)
            return null;
        
        Status = TimedApprovalSagaStatus.Denied;
        return new ConsoleWriteSideEffect($"Saga {Id} was denied");
    }
    
    public ConsoleWriteSideEffect Handle(StopTimedApprovalSaga _)
    {
        var isDisallowed = Status != TimedApprovalSagaStatus.Approved && Status != TimedApprovalSagaStatus.Denied;
        if (isDisallowed)
            return null;
        
        EndedAtUtc = DateTimeOffset.UtcNow;
        Status = TimedApprovalSagaStatus.Completed;
        MarkCompleted();
        return new ConsoleWriteSideEffect($"Saga {Id} was completed");
    }
    
    public ConsoleWriteSideEffect Handle(TimedApprovalSagaTimedOut _)
    {
        if (Status != TimedApprovalSagaStatus.Started)
            return null;
        
        EndedAtUtc = DateTimeOffset.UtcNow;
        Status = TimedApprovalSagaStatus.TimedOut;
        MarkCompleted();
        return new ConsoleWriteSideEffect($"Saga {Id} timed out");
    }
}




