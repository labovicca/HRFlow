using AutoMapper;
using Dapper;
using EmployeeService.Common.Data;
using EmployeeService.Common.DTOs.Employee;
using EmployeeService.Common.Entities;
using EmployeeService.Common.Enums;
using Npgsql;


namespace EmployeeService.Common.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly IEmployeeContext _context;
    private readonly IMapper _mapper;
    

    private async Task<string> GenerateEmployeeNumberAsync(NpgsqlConnection connection)
    
    {
        const string sql = "SELECT nextval('employee_number_seq')";
        var nextVal = await connection.ExecuteScalarAsync<long>(sql);
        return $"EMP-{nextVal:D6}";
    }


    public EmployeeRepository(IEmployeeContext context, IMapper mapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Employee WHERE Id = @Id";
        var employee = await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { Id = id });
        return _mapper.Map<EmployeeDto?>(employee);
    }

    public async Task<EmployeeDto?> GetByEmployeeNumberAsync(string employeeNumber)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Employee WHERE EmployeeNumber = @EmployeeNumber";
        var employee = await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { EmployeeNumber = employeeNumber });
        return _mapper.Map<EmployeeDto?>(employee);
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Employee";
        var employees = await connection.QueryAsync<Employee>(sql);
        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<IEnumerable<EmployeeDto>> GetByDepartmentAsync(string department)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Employee WHERE Department = @Department";
        var employees = await connection.QueryAsync<Employee>(sql, new { Department = department });
        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<IEnumerable<EmployeeDto>> GetByManagerIdAsync(int managerId)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Employee WHERE ManagerId = @ManagerId";
        var employees = await connection.QueryAsync<Employee>(sql, new { ManagerId = managerId });
        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<IEnumerable<EmployeeDto>> GetByStatusAsync(EmploymentStatus status)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Employee WHERE EmploymentStatus = @Status";
        var employees = await connection.QueryAsync<Employee>(sql, new { Status = (int)status });
        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<EmployeeDto?> CreateAsync(CreateEmployeeDto dto)
    {
        using var connection = _context.GetConnection();

        var employeeNumber = await GenerateEmployeeNumberAsync(connection);

        const string sql = """
                               INSERT INTO Employee 
                               (EmployeeNumber, FirstName, LastName, WorkEmail, PersonalEmail, PhoneNumber, DateOfBirth, JMBG,
                                Department, Position, ManagerId, EmploymentType, EmploymentStatus, HireDate, ProbationEndDate)
                               VALUES 
                               (@EmployeeNumber, @FirstName, @LastName, @WorkEmail, @PersonalEmail, @PhoneNumber, @DateOfBirth, @JMBG,
                                @Department, @Position, @ManagerId, @EmploymentType, @EmploymentStatus, @HireDate, @ProbationEndDate)
                               RETURNING *;
                           """;

        var createdEmployee = await connection.QuerySingleOrDefaultAsync<Employee>(sql, new
        {
            EmployeeNumber = employeeNumber,
            dto.FirstName,
            dto.LastName,
            dto.WorkEmail,
            dto.PersonalEmail,
            dto.PhoneNumber,
            dto.DateOfBirth,
            dto.JMBG,
            dto.Department,
            dto.Position,
            dto.ManagerId,
            EmploymentType = (int)dto.EmploymentType,
            EmploymentStatus = (int)dto.EmploymentStatus,
            dto.HireDate,
            dto.ProbationEndDate
        });

        if (createdEmployee == null)
            return null;

        return _mapper.Map<EmployeeDto>(createdEmployee);
    }
    public async Task<bool> UpdateAsync(UpdateEmployeeDto dto)
    {
        using var connection = _context.GetConnection();
        const string sql = """
            UPDATE Employee
            SET FirstName=@FirstName, LastName=@LastName, WorkEmail=@WorkEmail, PersonalEmail=@PersonalEmail,
                PhoneNumber=@PhoneNumber, DateOfBirth=@DateOfBirth, JMBG=@JMBG,
                Department=@Department, Position=@Position, ManagerId=@ManagerId,
                EmploymentType=@EmploymentType, EmploymentStatus=@EmploymentStatus,
                TerminationDate=@TerminationDate, ProbationEndDate=@ProbationEndDate
            WHERE Id=@Id
        """;

        var affected = await connection.ExecuteAsync(sql, new
        {
            dto.Id,
            dto.FirstName,
            dto.LastName,
            dto.WorkEmail,
            dto.PersonalEmail,
            dto.PhoneNumber,
            dto.DateOfBirth,
            dto.JMBG,
            dto.Department,
            dto.Position,
            dto.ManagerId,
            EmploymentType = (int)dto.EmploymentType,
            EmploymentStatus = (int)dto.EmploymentStatus,
            dto.TerminationDate,
            dto.ProbationEndDate
        });
        
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _context.GetConnection();
        const string sql = "DELETE FROM Employee WHERE Id=@Id";
        var affected = await connection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }
    
    public async Task<bool> UpdateStatusAsync(int employeeId, EmploymentStatus status)
    {
        using var connection = _context.GetConnection();

        const string sql = """
                               UPDATE Employee
                               SET EmploymentStatus = @Status
                               WHERE Id = @Id
                           """;

        var affected = await connection.ExecuteAsync(sql, new
        {
            Id = employeeId,
            Status = (int)status
        });

        return affected > 0;
    }
    
    public async Task<IEnumerable<EmployeeDto>> GetByTypeAsync(EmploymentType type)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Employee WHERE EmploymentType = @Type";
        var employees = await connection.QueryAsync<Employee>(sql, new { Type = (int)type });
        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<bool> UpdateTypeAsync(int employeeId, EmploymentType type)
    {
        using var connection = _context.GetConnection();

        const string sql = """
                               UPDATE Employee
                               SET EmploymentType = @Type
                               WHERE Id = @Id
                           """;

        var affected = await connection.ExecuteAsync(sql, new
        {
            Id = employeeId,
            Type = (int)type
        });

        return affected > 0;
    }
    
    public async Task<bool> TerminateAsync(int employeeId, DateTime terminationDate)
    {
        using var connection = _context.GetConnection();
        const string checkSql = """
                                    SELECT HireDate FROM Employee WHERE Id = @Id
                                """;
    
        var hireDate = await connection.QuerySingleOrDefaultAsync<DateTime?>(checkSql, new { Id = employeeId });
    
        if (hireDate == null)
            return false;
    
        if (terminationDate < hireDate.Value)
            throw new ArgumentException("Termination date cannot be before hire date");
        
        const string sql = """
                               UPDATE Employee
                               SET TerminationDate = @TerminationDate,
                                   EmploymentStatus = @Status
                               WHERE Id = @Id
                           """;
    
        var affected = await connection.ExecuteAsync(sql, new
        {
            Id = employeeId,
            TerminationDate = terminationDate,
            Status = (int)EmploymentStatus.Terminated
        });
    
        return affected > 0;
    }

}