using Microsoft.Extensions.Configuration;
using Npgsql;

namespace EmployeeService.Common.Data
{
    public class EmployeeContext : IEmployeeContext
    {
        private readonly IConfiguration _configuration;

        public EmployeeContext(IConfiguration configuration)
        {
            _configuration = configuration
                             ?? throw new ArgumentNullException(nameof(configuration));
        }

        public NpgsqlConnection GetConnection()
        {
            var conStr = _configuration.GetValue<string>("DatabaseSettings:ConnectionString");
            return new NpgsqlConnection(conStr);
        }
    }
}        