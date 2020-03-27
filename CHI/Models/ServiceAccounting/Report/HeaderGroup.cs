using CHI.Models.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class HeaderGroup : IOrderedHierarchical<HeaderGroup>
    {
        public string Name { get; set; }
        public string SubName { get; set; } = string.Empty;

        public bool IsRoot { get; set; } = false;
        public int Order { get; set; }
        public int Position { get; set; } = -1;

        public List<HeaderItem> HeaderItems { get; set; }

        public HeaderGroup Parent { get; set; }
        public List<HeaderGroup> Childs { get; set; }

        public HeaderGroup()
        {
            Childs = new List<HeaderGroup>();
            HeaderItems = new List<HeaderItem>();            
        }

        public static HeaderGroup CreateHeadersRecursive(HeaderGroup parent, Component component)
        {
            var header = new HeaderGroup
            {
                Name = component.Name,
                Order = component.Order,
                IsRoot = component.IsRoot,
                Parent = parent,
            };
            header.HeaderItems = HeaderItem.CreateHeaderItems(header, component.Indicators);

            parent?.Childs.Add(header);

            if (component.Childs?.Any() ?? false)
                foreach (var child in component.Childs)
                    CreateHeadersRecursive(header, child);

            return header;
        }

        public static HeaderGroup CreateHeadersRecursive(HeaderGroup parent, Department department)
        {
            var header = new HeaderGroup
            {
                Name = department.Name,
                Order = department.Order,
                IsRoot = department.IsRoot,
                Parent = parent
            };
            header.HeaderItems = HeaderItem.CreateHeaderItems(header, department.Parameters);

            parent?.Childs.Add(header);

            if (department.Childs?.Any() ?? false)
                foreach (var child in department.Childs)
                    CreateHeadersRecursive(header, child);

            if (department.Employees?.Any() ?? false)
                foreach (var employee in department.Employees)
                    CreateHeadersRecursive(header, employee);

            return header;
        }

        public static HeaderGroup CreateHeadersRecursive(HeaderGroup parent, Employee employee)
        {
            var header = new HeaderGroup
            {
                Name = employee.Medic.FullName,
                SubName = employee.Specialty.Name,
                Order = employee.Order,
                Parent = parent
            };
            header.HeaderItems = HeaderItem.CreateHeaderItems(header, employee.Parameters);

            parent?.Childs.Add(header);

            return header;
        }
    }
}
