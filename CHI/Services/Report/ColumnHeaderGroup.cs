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
        public int Position { get; set; } = -1;
        public Component Component { get; set; }
        public List<double> TreatmentCodes { get; set; }
        public List<double> VisitCodes { get; set; }
        public List<double> ContainsServiceCodes { get; set; }
        public List<double> NotContainsServiceCodes { get; set; }
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

            var groupedFilters = component.CaseFilters
                .OrderBy(x => x.Kind)
                .GroupBy(x => x.Kind)
                .Select(x => new { x.Key, Codes = x.Select(y => y.Code).ToList() });

            TreatmentCodes = groupedFilters.FirstOrDefault(x => x.Key == CaseFilterKind.TreatmentPurpose)?.Codes ?? new List<double>();
            VisitCodes = groupedFilters.FirstOrDefault(x => x.Key == CaseFilterKind.VisitPurpose)?.Codes ?? new List<double>();
            ContainsServiceCodes = groupedFilters.FirstOrDefault(x => x.Key == CaseFilterKind.ContainsService)?.Codes ?? new List<double>();
            NotContainsServiceCodes = groupedFilters.FirstOrDefault(x => x.Key == CaseFilterKind.NotContainsService)?.Codes ?? new List<double>();

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
