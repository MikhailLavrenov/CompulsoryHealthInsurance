using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Models.ServiceAccounting
{
    public class Department
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<Employee> Employees { get; set; }

        public Department()
        {
            Employees = new List<Employee>();
        }
    }
}
