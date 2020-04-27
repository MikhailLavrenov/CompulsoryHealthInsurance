using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Models.ServiceAccounting
{
    public class PlanningPermision
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public PlanningPermision()
        {
        }
        public PlanningPermision(User user, Department department)
        {
            User = user;
            Department = department;
        }
    }
}
