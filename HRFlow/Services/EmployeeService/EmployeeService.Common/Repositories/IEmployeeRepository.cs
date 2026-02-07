using EmployeeService.Common.DTOs.Employee;
using EmployeeService.Common.Enums;

namespace EmployeeService.Common.Repositories;

public interface IEmployeeRepository
{
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<EmployeeDto?> GetByEmployeeNumberAsync(string employeeNumber);
    Task<IEnumerable<EmployeeDto>> GetAllAsync();
    Task<IEnumerable<EmployeeDto>> GetByDepartmentAsync(string department);
    Task<IEnumerable<EmployeeDto>> GetByManagerIdAsync(int managerId);
    Task<IEnumerable<EmployeeDto>> GetByStatusAsync(EmploymentStatus status);
    Task<IEnumerable<EmployeeDto>> GetByTypeAsync(EmploymentType type);
    
    Task<EmployeeDto?> CreateAsync(CreateEmployeeDto employee);
    Task<bool> UpdateAsync(UpdateEmployeeDto employee);
    Task<bool> DeleteAsync(int id);
    
    Task<bool> UpdateStatusAsync(int employeeId, EmploymentStatus status);
    Task<bool> UpdateTypeAsync(int employeeId, EmploymentType type);
    
    Task<bool> TerminateAsync(int employeeId, DateTime terminationDate);
}