using EmployeeService.Common.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EmployeeService.API.HealthChecks;

public class PostgresHealthCheck : IHealthCheck
{
    private readonly IEmployeeContext _context;

    public PostgresHealthCheck(IEmployeeContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = _context.GetConnection();
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("PostgreSQL connection is healthy.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL connection is unhealthy.", ex);
        }
    }
}
