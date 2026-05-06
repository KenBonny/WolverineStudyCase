namespace WolverineCaseStudyElia.Host.Sagas;

public sealed class TimedApprovalSagaOptions
{
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(2);
}

