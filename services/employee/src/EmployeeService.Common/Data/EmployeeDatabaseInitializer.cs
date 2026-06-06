using Dapper;

namespace EmployeeService.Common.Data;

public class EmployeeDatabaseInitializer : IEmployeeDatabaseInitializer
{
    private readonly IEmployeeContext _context;

    public EmployeeDatabaseInitializer(IEmployeeContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task InitializeAsync()
    {
        using var connection = _context.GetConnection();
        await connection.OpenAsync();

        const string sql = """
            CREATE SEQUENCE IF NOT EXISTS employee_number_seq START 1;

            CREATE TABLE IF NOT EXISTS Employee (
                Id SERIAL PRIMARY KEY,
                EmployeeNumber VARCHAR(20) NOT NULL UNIQUE,
                FirstName VARCHAR(100) NOT NULL,
                LastName VARCHAR(100) NOT NULL,
                WorkEmail VARCHAR(255) NOT NULL UNIQUE,
                PersonalEmail VARCHAR(255) NOT NULL,
                PhoneNumber VARCHAR(50) NOT NULL,
                DateOfBirth TIMESTAMP NOT NULL,
                JMBG VARCHAR(13) NOT NULL UNIQUE,
                Department VARCHAR(150) NOT NULL,
                Position VARCHAR(150) NOT NULL,
                ManagerId INTEGER NULL REFERENCES Employee(Id),
                EmploymentType INTEGER NOT NULL,
                EmploymentStatus INTEGER NOT NULL,
                HireDate TIMESTAMP NOT NULL,
                TerminationDate TIMESTAMP NULL,
                ProbationEndDate TIMESTAMP NULL,
                CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
                UpdatedAt TIMESTAMP NULL,
                DeletedAt TIMESTAMP NULL,
                CreatedBy VARCHAR(100) NOT NULL DEFAULT 'system',
                UpdatedBy VARCHAR(100) NULL,
                DeletedBy VARCHAR(100) NULL,
                IsDeleted BOOLEAN NOT NULL DEFAULT FALSE
            );

            CREATE TABLE IF NOT EXISTS Document (
                Id SERIAL PRIMARY KEY,
                EmployeeId INTEGER NOT NULL REFERENCES Employee(Id) ON DELETE CASCADE,
                Name VARCHAR(200) NOT NULL,
                FilePath VARCHAR(500) NOT NULL,
                Type INTEGER NOT NULL,
                Status INTEGER NOT NULL,
                ExpirationDate TIMESTAMP NULL,
                OriginalFileName VARCHAR(255) NOT NULL DEFAULT '',
                ContentType VARCHAR(150) NOT NULL DEFAULT 'application/octet-stream',
                FileSize BIGINT NOT NULL DEFAULT 0,
                UploadedAt TIMESTAMP NOT NULL DEFAULT NOW(),
                CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
                UpdatedAt TIMESTAMP NULL,
                DeletedAt TIMESTAMP NULL,
                CreatedBy VARCHAR(100) NOT NULL DEFAULT 'system',
                UpdatedBy VARCHAR(100) NULL,
                DeletedBy VARCHAR(100) NULL,
                IsDeleted BOOLEAN NOT NULL DEFAULT FALSE
            );
            
            CREATE INDEX IF NOT EXISTS IX_Employee_Department ON Employee(Department);
            CREATE INDEX IF NOT EXISTS IX_Employee_ManagerId ON Employee(ManagerId);
            CREATE INDEX IF NOT EXISTS IX_Employee_Status ON Employee(EmploymentStatus);
            CREATE INDEX IF NOT EXISTS IX_Employee_IsDeleted ON Employee(IsDeleted);
            CREATE INDEX IF NOT EXISTS IX_Document_EmployeeId ON Document(EmployeeId);
            CREATE INDEX IF NOT EXISTS IX_Document_Status ON Document(Status);
            CREATE INDEX IF NOT EXISTS IX_Document_ExpirationDate ON Document(ExpirationDate);
            CREATE INDEX IF NOT EXISTS IX_Document_IsDeleted ON Document(IsDeleted);
            """;

        await connection.ExecuteAsync(sql);
    }
}
