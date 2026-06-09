using EmployeeService.Common.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EmployeeService.API.HealthChecks;

public class PostgresHealthCheck : IHealthCheck
{
    private readonly EmployeeDbContext _context;

    public PostgresHealthCheck(EmployeeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("PostgreSQL connection is healthy.")
                : HealthCheckResult.Unhealthy("PostgreSQL connection is unhealthy.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL connection is unhealthy.", ex);
        }
    }
}
