using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Lib_HealthChecks
{
    public class HeartBeat : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}