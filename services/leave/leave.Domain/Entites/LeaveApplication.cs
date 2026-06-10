using leave.Domain.Enums;
using leave.Domain.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace leave.Domain.Entites
{
    public class LeaveApplication
    {
        int employeeId { get; set; }
        int managerId { get; set; }
        int hrId { get; set; }
        DateRange? range { get; set; }
        LeaveType leaveType { get; set; }
        LeaveStatus leaveStatus { get; set; }
    }
}
