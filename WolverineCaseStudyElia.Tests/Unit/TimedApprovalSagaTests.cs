using Wolverine;
using WolverineCaseStudyElia.Contracts;
using WolverineCaseStudyElia.Host.Sagas;

namespace WolverineCaseStudyElia.Tests.Unit;

public class TimedApprovalSagaTests
{
    private readonly TimedApprovalSagaOptions _options = new()
    {
        Timeout = TimeSpan.FromMinutes(2)
    };
    private readonly Guid _id = Guid.NewGuid();
    
    [Fact]
    public void start_creates_started_state()
    {
        var (saga, consoleWriteSideEffect, outgoingMessages) = TimedApprovalSaga.Start(new StartTimedApprovalSaga(_id), _options);

        Assert.Equal(_id, saga.Id);
        Assert.Equal(TimedApprovalSagaStatus.Started, saga.Status);
        Assert.False(saga.IsCompleted());
        Assert.NotNull(consoleWriteSideEffect);
        Assert.NotEmpty(outgoingMessages);
    }

    [Fact]
    public void stop_marks_saga_as_completed()
    {
        var (saga, _, _) = TimedApprovalSaga.Start(new(_id), _options);
        var (approveMessage, _) = saga.Handle(new ApproveTimedApprovalSaga(saga.Id));
        Assert.Equal($"Saga {saga.Id} was approved", approveMessage.Text);

        var sideEffect = saga.Handle(new StopTimedApprovalSaga(saga.Id));

        Assert.Equal(TimedApprovalSagaStatus.Completed, saga.Status);
        Assert.True(saga.IsCompleted());
        Assert.Equal($"Saga {saga.Id} was completed", sideEffect.Text);
    }

    [Fact]
    public void timeout_marks_saga_as_completed()
    {
        var (saga, _, _) = TimedApprovalSaga.Start(new StartTimedApprovalSaga(_id), _options);

        var sideEffect = saga.Handle(new TimedApprovalSagaTimedOut(saga.Id));

        Assert.Equal(TimedApprovalSagaStatus.TimedOut, saga.Status);
        Assert.True(saga.IsCompleted());
        Assert.Equal($"Saga {saga.Id} timed out", sideEffect.Text);
    }
}

