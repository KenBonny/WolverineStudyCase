using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Wolverine;
using WolverineCaseStudy.Host.Sagas;

namespace WolverineCaseStudy.Tests.Integration;

public sealed class SagaWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TimedApprovalSaga:Timeout"] = "00:00:01"
            });
        });
    }

    /// <inheritdoc />
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseWolverine(options =>
        {
            // options.Discovery.DisableConventionalDiscovery().IncludeType<TimedApprovalSaga>();
            options.Policies.AutoApplyTransactions();
        });
        return base.CreateHost(builder);
    }
}

