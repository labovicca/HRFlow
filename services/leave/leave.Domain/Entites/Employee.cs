using System;
using System.Collections.Generic;
using System.Text;

namespace leave.Domain.Entites
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Department { get; set; }

        // EF Core requires a parameterless constructor to materialize entities
        private Employee() { }

        public Employee(int id, string name, string email, string department)
        {
            Id = id;
            Name = name;
            Email = email;
            Department = department;
        }
    }
}
