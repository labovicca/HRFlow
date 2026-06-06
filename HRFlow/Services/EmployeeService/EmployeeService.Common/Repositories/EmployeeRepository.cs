using AutoMapper;
using Dapper;
using EmployeeService.Common.Data;
using EmployeeService.Common.DTOs;
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
        const string sql = "SELECT * FROM Employee WHERE Id = @Id AND IsDeleted = FALSE";
        var employee = await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { Id = id });
        return _mapper.Map<EmployeeDto?>(employee);
    }

    public async Task<EmployeeDto?> GetByEmployeeNumberAsync(string employeeNumber)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Employee WHERE EmployeeNumber = @EmployeeNumber AND IsDeleted = FALSE";
        var employee = await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { EmployeeNumber = employeeNumber });
        return _mapper.Map<EmployeeDto?>(employee);
    }

    public async Task<EmployeeForPayrollDto?> GetForPayrollAsync(int id)
    {
        using var connection = _context.GetConnection();
        const string sql = """
            SELECT
                Id,
                EmployeeNumber,
                FirstName,
                LastName,
                Department,
                Position,
                EmploymentType,
                EmploymentStatus,
                HireDate,
                TerminationDate
            FROM Employee
            WHERE Id = @Id AND IsDeleted = FALSE
        """;

        var employee = await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { Id = id });
        return _mapper.Map<EmployeeForPayrollDto?>(employee);
    }

    public async Task<IEnumerable<EmployeeForPayrollDto>> GetForPayrollAsync(IEnumerable<int> ids)
    {
        var employeeIds = ids.Distinct().ToArray();
        if (employeeIds.Length == 0)
        {
            return Enumerable.Empty<EmployeeForPayrollDto>();
        }

        using var connection = _context.GetConnection();
        const string sql = """
            SELECT
                Id,
                EmployeeNumber,
                FirstName,
                LastName,
                Department,
                Position,
                EmploymentType,
                EmploymentStatus,
                HireDate,
                TerminationDate
            FROM Employee
            WHERE Id = ANY(@Ids) AND IsDeleted = FALSE
        """;

        var employees = await connection.QueryAsync<Employee>(sql, new { Ids = employeeIds });
        return _mapper.Map<IEnumerable<EmployeeForPayrollDto>>(employees);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT EXISTS(SELECT 1 FROM Employee WHERE Id = @Id AND IsDeleted = FALSE)";
        return await connection.ExecuteScalarAsync<bool>(sql, new { Id = id });
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Employee WHERE IsDeleted = FALSE";
        var employees = await connection.QueryAsync<Employee>(sql);
        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<IEnumerable<EmployeeDto>> GetByDepartmentAsync(string department)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Employee WHERE Department = @Department AND IsDeleted = FALSE";
        var employees = await connection.QueryAsync<Employee>(sql, new { Department = department });
        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<IEnumerable<EmployeeDto>> GetByManagerIdAsync(int managerId)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Employee WHERE ManagerId = @ManagerId AND IsDeleted = FALSE";
        var employees = await connection.QueryAsync<Employee>(sql, new { ManagerId = managerId });
        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<IEnumerable<EmployeeDto>> GetByStatusAsync(EmploymentStatus status)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Employee WHERE EmploymentStatus = @Status AND IsDeleted = FALSE";
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
                                Department, Position, ManagerId, EmploymentType, EmploymentStatus, HireDate, ProbationEndDate, CreatedAt, CreatedBy)
                               VALUES 
                               (@EmployeeNumber, @FirstName, @LastName, @WorkEmail, @PersonalEmail, @PhoneNumber, @DateOfBirth, @JMBG,
                                @Department, @Position, @ManagerId, @EmploymentType, @EmploymentStatus, @HireDate, @ProbationEndDate, @CreatedAt, @CreatedBy)
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
            dto.ProbationEndDate,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
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
                TerminationDate=@TerminationDate, ProbationEndDate=@ProbationEndDate,
                UpdatedAt=@UpdatedAt, UpdatedBy=@UpdatedBy
            WHERE Id=@Id AND IsDeleted = FALSE
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
            dto.ProbationEndDate,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "system"
        });
        
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _context.GetConnection();
        const string sql = """
            UPDATE Employee
            SET IsDeleted = TRUE,
                DeletedAt = @DeletedAt,
                DeletedBy = @DeletedBy
            WHERE Id = @Id AND IsDeleted = FALSE
        """;
        var affected = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = "system"
        });
        return affected > 0;
    }
    
    public async Task<bool> UpdateStatusAsync(int employeeId, EmploymentStatus status)
    {
        using var connection = _context.GetConnection();

        const string sql = """
                               UPDATE Employee
                               SET EmploymentStatus = @Status,
                                   UpdatedAt = @UpdatedAt,
                                   UpdatedBy = @UpdatedBy
                               WHERE Id = @Id AND IsDeleted = FALSE
                           """;

        var affected = await connection.ExecuteAsync(sql, new
        {
            Id = employeeId,
            Status = (int)status,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "system"
        });

        return affected > 0;
    }
    
    public async Task<IEnumerable<EmployeeDto>> GetByTypeAsync(EmploymentType type)
    {
        using var connection = _context.GetConnection();
        const string sql = "SELECT * FROM Employee WHERE EmploymentType = @Type AND IsDeleted = FALSE";
        var employees = await connection.QueryAsync<Employee>(sql, new { Type = (int)type });
        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<bool> UpdateTypeAsync(int employeeId, EmploymentType type)
    {
        using var connection = _context.GetConnection();

        const string sql = """
                               UPDATE Employee
                               SET EmploymentType = @Type,
                                   UpdatedAt = @UpdatedAt,
                                   UpdatedBy = @UpdatedBy
                               WHERE Id = @Id AND IsDeleted = FALSE
                           """;

        var affected = await connection.ExecuteAsync(sql, new
        {
            Id = employeeId,
            Type = (int)type,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "system"
        });

        return affected > 0;
    }
    
    public async Task<bool> TerminateAsync(int employeeId, DateTime terminationDate)
    {
        using var connection = _context.GetConnection();
        const string checkSql = """
                                    SELECT HireDate FROM Employee WHERE Id = @Id AND IsDeleted = FALSE
                                """;
    
        var hireDate = await connection.QuerySingleOrDefaultAsync<DateTime?>(checkSql, new { Id = employeeId });
    
        if (hireDate == null)
            return false;
    
        if (terminationDate < hireDate.Value)
            throw new ArgumentException("Termination date cannot be before hire date");
        
        const string sql = """
                               UPDATE Employee
                               SET TerminationDate = @TerminationDate,
                                   EmploymentStatus = @Status,
                                   UpdatedAt = @UpdatedAt,
                                   UpdatedBy = @UpdatedBy
                               WHERE Id = @Id AND IsDeleted = FALSE
                           """;
    
        var affected = await connection.ExecuteAsync(sql, new
        {
            Id = employeeId,
            TerminationDate = terminationDate,
            Status = (int)EmploymentStatus.Terminated,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "system"
        });
    
        return affected > 0;
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
        var offset = (page - 1) * pageSize;

        using var connection = _context.GetConnection();
        var parameters = new DynamicParameters();
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        var filters = new List<string> { "IsDeleted = FALSE" };

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add("""
                (
                    FirstName ILIKE @Search OR
                    LastName ILIKE @Search OR
                    WorkEmail ILIKE @Search OR
                    EmployeeNumber ILIKE @Search OR
                    Position ILIKE @Search
                )
            """);
            parameters.Add("Search", $"%{search.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            filters.Add("Department = @Department");
            parameters.Add("Department", department.Trim());
        }

        if (status.HasValue)
        {
            filters.Add("EmploymentStatus = @Status");
            parameters.Add("Status", (int)status.Value);
        }

        if (type.HasValue)
        {
            filters.Add("EmploymentType = @Type");
            parameters.Add("Type", (int)type.Value);
        }

        var whereClause = string.Join(" AND ", filters);
        var countSql = $"SELECT COUNT(*) FROM Employee WHERE {whereClause}";
        var dataSql = $"""
            SELECT *
            FROM Employee
            WHERE {whereClause}
            ORDER BY LastName, FirstName
            OFFSET @Offset LIMIT @PageSize
        """;

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var employees = await connection.QueryAsync<Employee>(dataSql, parameters);

        return new PagedResultDto<EmployeeDto>
        {
            Items = _mapper.Map<IReadOnlyCollection<EmployeeDto>>(employees),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

}