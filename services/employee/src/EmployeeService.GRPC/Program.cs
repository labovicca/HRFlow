using EmployeeService.Common.Data;
using EmployeeService.Common.Extensions;
using EmployeeService.GRPC.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddEmployeeServices();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IEmployeeDatabaseInitializer>();
    await initializer.InitializeAsync();
}

app.MapGrpcService<EmployeeGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
app.MapGet("/health", () => Results.Ok(new { service = "employee-grpc", status = "healthy" }));

app.Run();
