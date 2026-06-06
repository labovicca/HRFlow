using Npgsql;

namespace EmployeeService.Common.Data
{
    public interface IEmployeeContext
    {
        NpgsqlConnection GetConnection();
    }
}