using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payroll.Application.Interfaces;
using Payroll.Infrastructure.Messaging;
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
        services.AddScoped<IPayslipGenerator, PayslipPdfGenerator>();

        // Event Publishing
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        // MassTransit + RabbitMQ
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
                var rabbitMqPort = ushort.Parse(configuration["RabbitMQ:Port"] ?? "5672");
                var rabbitMqUser = configuration["RabbitMQ:Username"] ?? "guest";
                var rabbitMqPass = configuration["RabbitMQ:Password"] ?? "guest";

                cfg.Host(rabbitMqHost, rabbitMqPort, "/", h =>
                {
                    h.Username(rabbitMqUser);
                    h.Password(rabbitMqPass);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
