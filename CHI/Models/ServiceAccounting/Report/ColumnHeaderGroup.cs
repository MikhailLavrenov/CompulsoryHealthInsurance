using CHI.Models.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class ColumnHeaderGroup : IOrderedHierarchical<ColumnHeaderGroup>
    {
        public string Name { get; set; }
        public bool IsRoot { get; set; }
        public int Order { get; set; }
        public int Level { get; set; }
        public int Position { get; set; } = -1;
        public Component Component { get; set; }
        public List<CaseFilter> CaseFilters { get; set; }
        public List<ColumnHeaderItem> HeaderItems { get; set; }

        public ColumnHeaderGroup Parent { get; set; }
        public List<ColumnHeaderGroup> Childs { get; set; }



        public ColumnHeaderGroup(Component component, ColumnHeaderGroup parent)
        {
            Component = component;
            Name = component.Name;
            IsRoot = component.IsRoot;
            Order = component.Order;
            CaseFilters = component.CaseFilters.OrderBy(x => x.Kind).ToList();
            Parent = parent;
            Level = IsRoot ? -1 : parent.Level + 1;

            Childs = new List<ColumnHeaderGroup>();
            var HeaderItems = new List<ColumnHeaderItem>();

            if (component.Indicators?.Any() ?? false)
                foreach (var indicator in component.Indicators)
                    HeaderItems.Add(new ColumnHeaderItem(this, indicator));
        }



        public static ColumnHeaderGroup CreateHeadersRecursive(ColumnHeaderGroup parent, Component component)
        {
            var header = new ColumnHeaderGroup(component, parent);

            if (component.Childs?.Any() ?? false)
                foreach (var child in component.Childs)
                    header.Childs.Add(CreateHeadersRecursive(header, child));

            return header;
        }

    }
}
