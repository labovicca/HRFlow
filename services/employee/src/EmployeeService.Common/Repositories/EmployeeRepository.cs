using AutoMapper;
using EmployeeService.Common.Data;
using EmployeeService.Common.DTOs;
using EmployeeService.Common.DTOs.Employee;
using EmployeeService.Common.Entities;
using EmployeeService.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Common.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly EmployeeDbContext _context;
    private readonly IMapper _mapper;

    public EmployeeRepository(EmployeeDbContext context, IMapper mapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var employee = await ActiveEmployees()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        return _mapper.Map<EmployeeDto?>(employee);
    }

    public async Task<EmployeeDto?> GetByEmployeeNumberAsync(string employeeNumber)
    {
        var employee = await ActiveEmployees()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeNumber == employeeNumber);

        return _mapper.Map<EmployeeDto?>(employee);
    }

    public async Task<EmployeeForPayrollDto?> GetForPayrollAsync(int id)
    {
        var employee = await ActiveEmployees()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        return _mapper.Map<EmployeeForPayrollDto?>(employee);
    }

    public async Task<IEnumerable<EmployeeForPayrollDto>> GetForPayrollAsync(IEnumerable<int> ids)
    {
        var employeeIds = ids.Distinct().ToArray();
        if (employeeIds.Length == 0)
        {
            return Enumerable.Empty<EmployeeForPayrollDto>();
        }

        var employees = await ActiveEmployees()
            .AsNoTracking()
            .Where(e => employeeIds.Contains(e.Id))
            .ToListAsync();

        return _mapper.Map<IEnumerable<EmployeeForPayrollDto>>(employees);
    }

    public Task<bool> ExistsAsync(int id)
    {
        return ActiveEmployees().AnyAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
    {
        var employees = await ActiveEmployees()
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<IEnumerable<EmployeeDto>> GetByDepartmentAsync(string department)
    {
        var employees = await ActiveEmployees()
            .AsNoTracking()
            .Where(e => e.Department == department)
            .ToListAsync();

        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<IEnumerable<EmployeeDto>> GetByManagerIdAsync(int managerId)
    {
        var employees = await ActiveEmployees()
            .AsNoTracking()
            .Where(e => e.ManagerId == managerId)
            .ToListAsync();

        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<LeaveApproversDto?> GetLeaveApproversAsync(int employeeId)
    {
        var employee = await ActiveEmployees()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employeeId);
        if (employee == null)
        {
            return null;
        }

        if (!employee.ManagerId.HasValue)
        {
            throw new InvalidOperationException($"Employee {employeeId} does not have a manager assigned");
        }

        var manager = await ActiveEmployees()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employee.ManagerId.Value);
        if (manager == null)
        {
            throw new InvalidOperationException($"Manager {employee.ManagerId.Value} is not an active employee");
        }

        var hr = await _context.DepartmentHrAssignments
            .AsNoTracking()
            .Where(a => a.Department == employee.Department)
            .Join(
                ActiveEmployees().AsNoTracking().Where(e => e.Role == EmployeeRole.Hr),
                assignment => assignment.HrEmployeeId,
                hrEmployee => hrEmployee.Id,
                (_, hrEmployee) => hrEmployee)
            .FirstOrDefaultAsync();
        if (hr == null)
        {
            throw new InvalidOperationException($"Department {employee.Department} does not have an active HR partner assigned");
        }

        return new LeaveApproversDto
        {
            EmployeeId = employee.Id,
            EmployeeNumber = employee.EmployeeNumber,
            ManagerId = manager.Id,
            ManagerEmployeeNumber = manager.EmployeeNumber,
            HrEmployeeId = hr.Id,
            HrEmployeeNumber = hr.EmployeeNumber,
            Department = employee.Department
        };
    }

    public async Task<IEnumerable<EmployeeDto>> GetByStatusAsync(EmploymentStatus status)
    {
        var employees = await ActiveEmployees()
            .AsNoTracking()
            .Where(e => e.EmploymentStatus == status)
            .ToListAsync();

        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<EmployeeDto?> CreateAsync(CreateEmployeeDto dto)
    {
        var employee = _mapper.Map<Employee>(dto);
        employee.EmployeeNumber = await GenerateEmployeeNumberAsync();
        employee.CreatedAt = DateTime.UtcNow;
        employee.CreatedBy = "system";
        employee.IsDeleted = false;

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return _mapper.Map<EmployeeDto>(employee);
    }

    public async Task<bool> UpdateAsync(UpdateEmployeeDto dto)
    {
        var employee = await ActiveEmployees().FirstOrDefaultAsync(e => e.Id == dto.Id);
        if (employee == null)
        {
            return false;
        }

        _mapper.Map(dto, employee);
        employee.UpdatedAt = DateTime.UtcNow;
        employee.UpdatedBy = "system";

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var employee = await ActiveEmployees().FirstOrDefaultAsync(e => e.Id == id);
        if (employee == null)
        {
            return false;
        }

        employee.IsDeleted = true;
        employee.DeletedAt = DateTime.UtcNow;
        employee.DeletedBy = "system";

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateStatusAsync(int employeeId, EmploymentStatus status)
    {
        var employee = await ActiveEmployees().FirstOrDefaultAsync(e => e.Id == employeeId);
        if (employee == null)
        {
            return false;
        }

        employee.EmploymentStatus = status;
        employee.UpdatedAt = DateTime.UtcNow;
        employee.UpdatedBy = "system";

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<EmployeeDto>> GetByTypeAsync(EmploymentType type)
    {
        var employees = await ActiveEmployees()
            .AsNoTracking()
            .Where(e => e.EmploymentType == type)
            .ToListAsync();

        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<bool> UpdateTypeAsync(int employeeId, EmploymentType type)
    {
        var employee = await ActiveEmployees().FirstOrDefaultAsync(e => e.Id == employeeId);
        if (employee == null)
        {
            return false;
        }

        employee.EmploymentType = type;
        employee.UpdatedAt = DateTime.UtcNow;
        employee.UpdatedBy = "system";

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateRoleAsync(int employeeId, EmployeeRole role)
    {
        var employee = await ActiveEmployees().FirstOrDefaultAsync(e => e.Id == employeeId);
        if (employee == null)
        {
            return false;
        }

        employee.Role = role;
        employee.UpdatedAt = DateTime.UtcNow;
        employee.UpdatedBy = "system";

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> SetDepartmentHrAsync(string department, int hrEmployeeId)
    {
        if (string.IsNullOrWhiteSpace(department))
        {
            throw new ArgumentException("Department is required", nameof(department));
        }

        var trimmedDepartment = department.Trim();
        var hrEmployee = await ActiveEmployees().FirstOrDefaultAsync(e => e.Id == hrEmployeeId);
        if (hrEmployee == null)
        {
            return false;
        }

        if (hrEmployee.Role != EmployeeRole.Hr)
        {
            throw new InvalidOperationException($"Employee {hrEmployeeId} must have HR role before being assigned as department HR");
        }

        var assignment = await _context.DepartmentHrAssignments
            .FirstOrDefaultAsync(a => a.Department == trimmedDepartment);
        if (assignment == null)
        {
            _context.DepartmentHrAssignments.Add(new DepartmentHrAssignment
            {
                Department = trimmedDepartment,
                HrEmployeeId = hrEmployeeId,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            assignment.HrEmployeeId = hrEmployeeId;
            assignment.UpdatedAt = DateTime.UtcNow;
        }

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> TerminateAsync(int employeeId, DateTime terminationDate)
    {
        var employee = await ActiveEmployees().FirstOrDefaultAsync(e => e.Id == employeeId);
        if (employee == null)
        {
            return false;
        }

        if (terminationDate < employee.HireDate)
        {
            throw new ArgumentException("Termination date cannot be before hire date");
        }

        employee.TerminationDate = terminationDate;
        employee.EmploymentStatus = EmploymentStatus.Terminated;
        employee.UpdatedAt = DateTime.UtcNow;
        employee.UpdatedBy = "system";

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<PagedResultDto<EmployeeDto>> SearchAsync(
        string? search,
        string? department,
        EmploymentStatus? status,
        EmploymentType? type,
        int page,
        int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = ActiveEmployees().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(e =>
                EF.Functions.ILike(e.FirstName, pattern) ||
                EF.Functions.ILike(e.LastName, pattern) ||
                EF.Functions.ILike(e.WorkEmail, pattern) ||
                EF.Functions.ILike(e.EmployeeNumber, pattern) ||
                EF.Functions.ILike(e.Position, pattern));
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            var trimmedDepartment = department.Trim();
            query = query.Where(e => e.Department == trimmedDepartment);
        }

        if (status.HasValue)
        {
            query = query.Where(e => e.EmploymentStatus == status.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(e => e.EmploymentType == type.Value);
        }

        var totalCount = await query.CountAsync();
        var employees = await query
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<EmployeeDto>
        {
            Items = _mapper.Map<IReadOnlyCollection<EmployeeDto>>(employees),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private IQueryable<Employee> ActiveEmployees()
    {
        return _context.Employees.Where(e => !e.IsDeleted);
    }

    private async Task<string> GenerateEmployeeNumberAsync()
    {
        var lastEmployeeNumber = await _context.Employees
            .Where(e => e.EmployeeNumber.StartsWith("EMP-"))
            .OrderByDescending(e => e.Id)
            .Select(e => e.EmployeeNumber)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (!string.IsNullOrWhiteSpace(lastEmployeeNumber) &&
            int.TryParse(lastEmployeeNumber["EMP-".Length..], out var currentNumber))
        {
            nextNumber = currentNumber + 1;
        }

        return $"EMP-{nextNumber:D6}";
    }
}
