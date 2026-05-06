namespace WolverineCaseStudyElia.Contracts;

public enum SagaAction
{
    Start,
    Approve,
    Deny,
    Stop
}

public enum TimedApprovalSagaStatus
{
    Started,
    Approved,
    Denied,
    TimedOut,
    Completed
}

public record StartTimedApprovalSaga(Guid Id);

public record ApproveTimedApprovalSaga(Guid Id);

public record DenyTimedApprovalSaga(Guid Id);

public record StopTimedApprovalSaga(Guid Id);

public record TimedApprovalSagaTimedOut(Guid Id);

