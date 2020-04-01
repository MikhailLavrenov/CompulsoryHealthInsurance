using CHI.Models.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class RowHeaderGroup : IOrderedHierarchical<RowHeaderGroup>
    {
        public string Name { get; set; }
        public string SubName { get; set; }
        public bool IsRoot { get; set; }
        public int Order { get; set; }
        public int Level { get; set; }
        public Department Department { get; set; }
        public Employee Employee { get; set; }
        public List<RowHeaderItem> HeaderItems { get; set; }
        public List<Case> FactCases { get; set; }

        public RowHeaderGroup Parent { get; set; }
        public List<RowHeaderGroup> Childs { get; set; }



        public RowHeaderGroup(Department department, RowHeaderGroup parent)
        {
            Name = department.Name;
            SubName = string.Empty;
            Order = department.Order;
            IsRoot = department.IsRoot;            
            Parent = parent;
            Level = IsRoot ? -1 : parent.Level + 1;

            Childs = new List<RowHeaderGroup>();

            SetHeaderItems(department.Parameters);
        }

        public RowHeaderGroup(Employee employee, RowHeaderGroup parent)
        {
            Name = employee.Medic.FullName;
            SubName = employee.Specialty.Name;
            Order = employee.Order;
            IsRoot = false;
            Parent = parent;
            Level = parent.Level + 1;

            Childs = new List<RowHeaderGroup>();

            SetHeaderItems(employee.Parameters);
        }



        public static RowHeaderGroup CreateHeadersRecursive(RowHeaderGroup parent, Department department)
        {
            var header = new RowHeaderGroup(department, parent);

            if (department.Childs?.Any() ?? false)
                foreach (var child in department.Childs)
                    header.Childs.Add(CreateHeadersRecursive(header, child));
            else if (department.Employees?.Any() ?? false)
                foreach (var employee in department.Employees)
                    header.Childs.Add(new RowHeaderGroup(employee, parent));

            return header;
        }

        private void SetHeaderItems(List<Parameter> parameters)
        {
            HeaderItems = new List<RowHeaderItem>();

            if (parameters?.Any() ?? false)
                foreach (var parameter in parameters)
                    HeaderItems.Add(new RowHeaderItem(this, parameter));
        }
    }
}
