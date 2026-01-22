using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payroll.Application.Interfaces;
using Payroll.Infrastructure.Persistence;
using Payroll.Infrastructure.Repositories;
using Payroll.Infrastructure.Services;

namespace Payroll.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<PayrollDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("PayrollDb"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly("Payroll.Infrastructure")
            ));

        // Repositories
        services.AddScoped<IPayrollRunRepository, PayrollRunRepository>();
        services.AddScoped<IPayrollConfigurationRepository, PayrollConfigurationRepository>();

        // Services
        services.AddScoped<IPayrollCalculator, PayrollCalculator>();

        return services;
    }
}
