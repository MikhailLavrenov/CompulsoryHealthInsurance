using CHI.Models.Infrastructure;
using System.Collections.Generic;

namespace CHI.Models.ServiceAccounting
{
    public class Department: IOrderedHierarchical<Department>
    {
        public static string UnknownTitle { get; } = "Неизвестно";

        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }

        public bool IsRoot { get; set; }

        public List<Employee> Employees { get; set; }

        public Department Parent { get; set; }
        public List<Department> Childs { get; set; }

        public Department()
        {
            Employees = new List<Employee>();
        }

        public Department(string name)
        {
            Name = name;
        }
    }
}
