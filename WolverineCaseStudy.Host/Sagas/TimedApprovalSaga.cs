using Microsoft.Extensions.Options;
using Wolverine;
using WolverineCaseStudy.Contracts;

namespace WolverineCaseStudy.Host.Sagas;

public class TimedApprovalSaga : Wolverine.Saga
{
    public Guid Id { get; init; }
    public TimedApprovalSagaStatus Status { get; private set; }
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset? EndedAtUtc { get; private set; }

    public static (TimedApprovalSaga saga, ConsoleWriteSideEffect, OutgoingMessages) Start(
        StartTimedApprovalSaga command,
        IOptions<TimedApprovalSagaOptions> options)
    {
        var saga = new TimedApprovalSaga
        {
            Id = command.Id,
            StartedAtUtc = DateTimeOffset.UtcNow,
            Status = TimedApprovalSagaStatus.Started
        };

        var messages = new OutgoingMessages([new TimedApprovalSagaTimedOut(command.Id).DelayedFor(options.Value.Timeout)]);

        return (saga, new ConsoleWriteSideEffect($"Saga {command.Id} started"), messages);
    }

    public (ConsoleWriteSideEffect?, OutgoingMessages messages) Handle(ApproveTimedApprovalSaga _)
    {
        var messages = new OutgoingMessages();
        if (Status != TimedApprovalSagaStatus.Started)
            return (null, messages);

        Status = TimedApprovalSagaStatus.Approved;
        messages.Add(new SagaApproved(Id));
        return (new ConsoleWriteSideEffect($"Saga {Id} was approved"), messages);
    }
    
    public (ConsoleWriteSideEffect?, OutgoingMessages messages) Handle(DenyTimedApprovalSaga _)
    {
        var messages = new OutgoingMessages();
        if (Status != TimedApprovalSagaStatus.Started)
            return (null, messages);
        
        Status = TimedApprovalSagaStatus.Denied;
        messages.Add(new SagaDenied(Id));
        return (new ConsoleWriteSideEffect($"Saga {Id} was denied"), messages);
    }
    
    public ConsoleWriteSideEffect? Handle(StopTimedApprovalSaga _)
    {
        var isDisallowed = Status != TimedApprovalSagaStatus.Approved && Status != TimedApprovalSagaStatus.Denied;
        if (isDisallowed)
            return null;
        
        EndedAtUtc = DateTimeOffset.UtcNow;
        Status = TimedApprovalSagaStatus.Completed;
        MarkCompleted();
        return new ConsoleWriteSideEffect($"Saga {Id} was completed");
    }
    
    public ConsoleWriteSideEffect? Handle(TimedApprovalSagaTimedOut _)
    {
        if (Status != TimedApprovalSagaStatus.Started)
            return null;
        
        EndedAtUtc = DateTimeOffset.UtcNow;
        Status = TimedApprovalSagaStatus.TimedOut;
        MarkCompleted();
        return new ConsoleWriteSideEffect($"Saga {Id} timed out");
    }
}
