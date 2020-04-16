using CHI.Models.Infrastructure;
using CHI.Models.ServiceAccounting;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace CHI.Services.Report
{
    public class ColumnHeaderGroup : BindableBase, IOrderedHierarchical<ColumnHeaderGroup>
    {
        bool? isCollapsed;
        bool isVisible=true;

        public string Name { get; set; }
        public bool IsRoot { get; set; }
        public int Order { get; set; }
        public int Level { get; set; }
        public int Index { get; set; }
        public Color Color { get; set; }
        public bool CanCollapse { get; private set; }
        public bool? IsCollapsed { get => isCollapsed; private set => SetProperty(ref isCollapsed, value); }
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (isVisible != value)
                {
                    SetProperty(ref isVisible, value);

                    if (!isVisible && IsCollapsed == false)
                        SwitchCollapseExecute();

                    IsVisibleChangedEvent(this, new EventArgs());
                }
            }
        }

        public List<ColumnHeaderItem> HeaderItems { get; set; }

        public Component Component { get; set; }
        public List<CaseFilter> TreatmentFilters { get; set; }
        public List<CaseFilter> VisitFilters { get; set; }
        public List<CaseFilter> ContainsServiceFilters { get; set; }
        public List<CaseFilter> NotContainsServiceFilters { get; set; }

        public ColumnHeaderGroup Parent { get; set; }
        public List<ColumnHeaderGroup> Childs { get; set; }

        public DelegateCommand SwitchCollapseCommand { get; }

        public event EventHandler IsVisibleChangedEvent;

        public ColumnHeaderGroup(Component component, ColumnHeaderGroup parent)
        {
            Component = component;
            Name = component.Name;
            IsRoot = component.IsRoot;
            Order = component.Order;
            Color = (Color)ColorConverter.ConvertFromString(component.HexColor);
            CanCollapse = component.Childs?.Any() ?? false;
            IsCollapsed = CanCollapse ? false : (bool?)null;
            IsVisible = true;



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

            SwitchCollapseCommand = new DelegateCommand(SwitchCollapseExecute, () => CanCollapse);
        }


        public static ColumnHeaderGroup CreateHeadersRecursive(ColumnHeaderGroup parent, Component component)
        {
            var header = new ColumnHeaderGroup(component, parent);

            if (component.Childs?.Any() ?? false)
                foreach (var child in component.Childs)
                    header.Childs.Add(CreateHeadersRecursive(header, child));

            return header;
        }

        private void SwitchCollapseExecute()
        {
            IsCollapsed = !IsCollapsed;

            foreach (var child in Childs)
                child.IsVisible = !IsCollapsed.Value;
        }
    }
}
