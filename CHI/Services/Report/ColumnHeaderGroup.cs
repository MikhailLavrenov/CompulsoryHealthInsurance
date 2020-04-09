using CHI.Models.Infrastructure;
using CHI.Models.ServiceAccounting;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services.Report
{
    public class ColumnHeaderGroup : IOrderedHierarchical<ColumnHeaderGroup>
    {
        public string Name { get; set; }
        public bool IsRoot { get; set; }
        public int Order { get; set; }
        public int Level { get; set; }
        public int Index { get; set; }
        public Component Component { get; set; }
        public List<CaseFilter> TreatmentFilters { get; set; }
        public List<CaseFilter> VisitFilters { get; set; }
        public List<CaseFilter> ContainsServiceFilters { get; set; }
        public List<CaseFilter> NotContainsServiceFilters { get; set; }
        public List<ColumnHeaderItem> HeaderItems { get; set; }

        public ColumnHeaderGroup Parent { get; set; }
        public List<ColumnHeaderGroup> Childs { get; set; }



        public ColumnHeaderGroup(Component component, ColumnHeaderGroup parent)
        {
            Component = component;
            Name = component.Name;
            IsRoot = component.IsRoot;
            Order = component.Order;

            Parent = parent;
            Level = IsRoot ? -1 : parent.Level + 1;

            var groupedFilters = component.CaseFilters.GroupBy(x => x.Kind).ToList();

            TreatmentFilters = groupedFilters.FirstOrDefault(x => x.Key == CaseFilterKind.TreatmentPurpose)?.ToList() ?? new List<CaseFilter>();
            VisitFilters = groupedFilters.FirstOrDefault(x => x.Key == CaseFilterKind.VisitPurpose)?.ToList() ?? new List<CaseFilter>();
            ContainsServiceFilters = groupedFilters.FirstOrDefault(x => x.Key == CaseFilterKind.ContainsService)?.ToList() ?? new List<CaseFilter>();
            NotContainsServiceFilters = groupedFilters.FirstOrDefault(x => x.Key == CaseFilterKind.NotContainsService)?.ToList() ?? new List<CaseFilter>();

            Childs = new List<ColumnHeaderGroup>();
            HeaderItems = new List<ColumnHeaderItem>();

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
