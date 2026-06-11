using EmployeeService.API.HealthChecks;
using EmployeeService.API.Middleware;
using EmployeeService.Common.Extensions;
using EmployeeService.Common.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddEmployeeServices(builder.Configuration);
builder.Services.AddDocumentServices();
builder.Services.AddHealthChecks()
    .AddCheck<PostgresHealthCheck>("postgresql", tags: new[] { "db", "ready" });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions())
    .WithName("Health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
}).WithName("Readiness");
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
}).WithName("Liveness");

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
