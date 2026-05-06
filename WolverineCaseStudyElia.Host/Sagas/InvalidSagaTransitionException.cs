using WolverineCaseStudyElia.Contracts;

namespace WolverineCaseStudyElia.Host.Sagas;

public sealed class InvalidSagaTransitionException(Guid sagaId, TimedApprovalSagaStatus status, string detail, SagaAction[] allowedActions)
    : Exception(detail)
{
    public Guid SagaId { get; } = sagaId;
    public TimedApprovalSagaStatus Status { get; } = status;
    public string Detail { get; } = detail;
    public SagaAction[] AllowedActions { get; } = allowedActions;
}

