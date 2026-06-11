using EmployeeService.Common.Data;
using EmployeeService.Common.Extensions;
using EmployeeService.GRPC.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddEmployeeServices(builder.Configuration);

var app = builder.Build();

app.MapGrpcService<EmployeeGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
app.MapGet("/health", () => Results.Ok(new { service = "employee-grpc", status = "healthy" }));

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
