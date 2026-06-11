using EmployeeService.Common.DTOs.Employee;
using EmployeeService.Common.DTOs;
using EmployeeService.Common.Enums;

namespace EmployeeService.Common.Repositories;

public interface IEmployeeRepository
{
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<EmployeeDto?> GetByEmployeeNumberAsync(string employeeNumber);
    Task<EmployeeForPayrollDto?> GetForPayrollAsync(int id);
    Task<IEnumerable<EmployeeForPayrollDto>> GetForPayrollAsync(IEnumerable<int> ids);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<EmployeeDto>> GetAllAsync();
    Task<IEnumerable<EmployeeDto>> GetByDepartmentAsync(string department);
    Task<IEnumerable<EmployeeDto>> GetByManagerIdAsync(int managerId);
    Task<LeaveApproversDto?> GetLeaveApproversAsync(int employeeId);
    Task<IEnumerable<EmployeeDto>> GetByStatusAsync(EmploymentStatus status);
    Task<IEnumerable<EmployeeDto>> GetByTypeAsync(EmploymentType type);
    Task<PagedResultDto<EmployeeDto>> SearchAsync(
        string? search,
        string? department,
        EmploymentStatus? status,
        EmploymentType? type,
        int page,
        int pageSize);
    
    Task<EmployeeDto?> CreateAsync(CreateEmployeeDto employee);
    Task<bool> UpdateAsync(UpdateEmployeeDto employee);
    Task<bool> DeleteAsync(int id);
    
    Task<bool> UpdateStatusAsync(int employeeId, EmploymentStatus status);
    Task<bool> UpdateTypeAsync(int employeeId, EmploymentType type);
    Task<bool> UpdateRoleAsync(int employeeId, EmployeeRole role);
    Task<bool> SetDepartmentHrAsync(string department, int hrEmployeeId);
    
    Task<bool> TerminateAsync(int employeeId, DateTime terminationDate);
}
