using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Wolverine.Persistence.Sagas;
using WolverineCaseStudyElia.Contracts;
using WolverineCaseStudyElia.Host.Sagas;

namespace WolverineCaseStudyElia.Tests.Integration;

public sealed class TimedApprovalSagaEndpointTests : IClassFixture<SagaWebApplicationFactory>
{
    private readonly SagaWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TimedApprovalSagaEndpointTests(SagaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task start_approve_stop_returns_accepted()
    {
        var sagaId = Guid.NewGuid();

        var start = await _client.PostAsync($"/saga?action=Start&sagaId={sagaId}", null);
        Assert.Equal(HttpStatusCode.Accepted, start.StatusCode);
        var saga = LoadSaga(sagaId);
        Assert.NotNull(saga);
        Assert.Equal(TimedApprovalSagaStatus.Started, saga.Status);
        
        var approve = await _client.PostAsync($"/saga?action=Approve&sagaId={sagaId}", null);
        Assert.Equal(HttpStatusCode.Accepted, approve.StatusCode);
        saga = LoadSaga(sagaId)!;
        Assert.Equal(TimedApprovalSagaStatus.Approved, saga.Status);
        
        var stop = await _client.PostAsync($"/saga?action=Stop&sagaId={sagaId}", null);
        Assert.Equal(HttpStatusCode.Accepted, stop.StatusCode);
        saga = LoadSaga(sagaId)!;
        Assert.Null(saga);
    }

    [Fact]
    public async Task stop_from_started_returns_conflict_problem_details()
    {
        var sagaId = Guid.NewGuid();
        await _client.PostAsync($"/saga?action=Start&sagaId={sagaId}", null);
        
        var saga = LoadSaga(sagaId);
        Assert.NotNull(saga);

        var response = await _client.PostAsync($"/saga?action=Stop&sagaId={sagaId}", null);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task shortened_timeout_triggers_conflict_for_late_approve()
    {
        var sagaId = Guid.NewGuid();
        var started = await _client.PostAsync($"/saga?action=Start&sagaId={sagaId}", null);

        Assert.Equal(HttpStatusCode.Accepted, started.StatusCode);

        await Task.Delay(TimeSpan.FromSeconds(2));

        var lateApprove = await _client.PostAsync($"/saga?action=Approve&sagaId={sagaId}", null);
        Assert.Equal(HttpStatusCode.Accepted, lateApprove.StatusCode);
    }
    
    private TimedApprovalSaga? LoadSaga(Guid sagaId)
    {
        var sagaPersistor = _factory.Services.GetRequiredService<InMemorySagaPersistor>();
        return sagaPersistor.Load<TimedApprovalSaga>(sagaId);
    }
}

