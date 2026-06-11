using EmployeeService.Common.DTOs.Employee;
using EmployeeService.Common.Repositories;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace EmployeeService.GRPC.Services;

public class EmployeeGrpcService : EmployeeProtoService.EmployeeProtoServiceBase
{
    private readonly IEmployeeRepository _repository;
    private readonly ILogger<EmployeeGrpcService> _logger;

    public EmployeeGrpcService(IEmployeeRepository repository, ILogger<EmployeeGrpcService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<EmployeeForPayrollResponse> GetEmployeeForPayroll(
        GetEmployeeForPayrollRequest request,
        ServerCallContext context)
    {
        var employee = await _repository.GetForPayrollAsync(request.EmployeeId);
        if (employee == null)
        {
            throw new RpcException(new Status(
                StatusCode.NotFound,
                $"Employee with ID {request.EmployeeId} not found"));
        }

        _logger.LogInformation("Retrieved employee payroll data for employee {EmployeeId}", request.EmployeeId);
        return MapToResponse(employee);
    }

    public override async Task<GetEmployeesForPayrollResponse> GetEmployeesForPayroll(
        GetEmployeesForPayrollRequest request,
        ServerCallContext context)
    {
        var employees = await _repository.GetForPayrollAsync(request.EmployeeIds);
        var response = new GetEmployeesForPayrollResponse();
        response.Employees.AddRange(employees.Select(MapToResponse));

        _logger.LogInformation("Retrieved payroll data for {Count} employee(s)", response.Employees.Count);
        return response;
    }

    public override async Task<EmployeeExistsResponse> EmployeeExists(
        EmployeeExistsRequest request,
        ServerCallContext context)
    {
        var exists = await _repository.ExistsAsync(request.EmployeeId);
        return new EmployeeExistsResponse { Exists = exists };
    }

    public override async Task<LeaveApproversResponse> GetLeaveApprovers(
        GetLeaveApproversRequest request,
        ServerCallContext context)
    {
        try
        {
            var approvers = await _repository.GetLeaveApproversAsync(request.EmployeeId);
            if (approvers == null)
            {
                throw new RpcException(new Status(
                    StatusCode.NotFound,
                    $"Employee with ID {request.EmployeeId} not found"));
            }

            _logger.LogInformation("Retrieved leave approvers for employee {EmployeeId}", request.EmployeeId);
            return new LeaveApproversResponse
            {
                EmployeeId = approvers.EmployeeId,
                EmployeeNumber = approvers.EmployeeNumber,
                ManagerId = approvers.ManagerId,
                ManagerEmployeeNumber = approvers.ManagerEmployeeNumber,
                HrEmployeeId = approvers.HrEmployeeId,
                HrEmployeeNumber = approvers.HrEmployeeNumber,
                Department = approvers.Department
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

    private static EmployeeForPayrollResponse MapToResponse(EmployeeForPayrollDto employee)
    {
        var response = new EmployeeForPayrollResponse
        {
            Id = employee.Id,
            EmployeeNumber = employee.EmployeeNumber,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Department = employee.Department,
            Position = employee.Position,
            EmploymentType = employee.EmploymentType,
            EmploymentStatus = employee.EmploymentStatus,
            HireDate = ToTimestamp(employee.HireDate)
        };

        if (employee.TerminationDate.HasValue)
        {
            response.TerminationDate = ToTimestamp(employee.TerminationDate.Value);
        }

        return response;
    }

    private static Timestamp ToTimestamp(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);

        return Timestamp.FromDateTime(utc);
    }
}
