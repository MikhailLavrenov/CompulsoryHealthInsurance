using CHI.Models.Infrastructure;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Windows.Media;

namespace CHI.Models.ServiceAccounting
{
    public class Department: BindableBase, IOrderedHierarchical<Department>
    {
        string hexColor = "#FFFFFF";

        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public string HexColor { get=> hexColor; set=>SetProperty(ref hexColor, value); }
        public List<Employee> Employees { get; set; }
        public List<Parameter> Parameters { get; set; }
        public bool IsRoot { get; set; }
        public Department Parent { get; set; }
        public List<Department> Childs { get; set; }
        public List<PlanningPermision> PlanningPermisions { get; set; }

        public Department()
        {
            Parameters = new List<Parameter>();
            Employees = new List<Employee>();
            Parameters = new List<Parameter>();
            PlanningPermisions = new List<PlanningPermision>();
        }

        public Department(string name)
        {
            Name = name;
        }
    }
}
