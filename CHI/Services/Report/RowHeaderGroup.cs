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
    public class RowHeaderGroup : BindableBase, IOrderedHierarchical<RowHeaderGroup>
    {
        bool? isCollapsed;
        bool isVisible = true;

        public string Name { get; set; }
        public string SubName { get; set; }
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
                if (isVisible == value)
                    return;

                SetProperty(ref isVisible, value);

                if (!isVisible && IsCollapsed == false)
                    SwitchCollapseExecute();

                IsVisibleChangedEvent(this, new EventArgs());
            }
        }
        public Department Department { get; set; }
        public Employee Employee { get; set; }
        public List<RowHeaderItem> HeaderItems { get; set; }
        public List<Case> FactCases { get; set; }
        public RowHeaderGroup Parent { get; set; }
        public List<RowHeaderGroup> Childs { get; set; }

        public DelegateCommand SwitchCollapseCommand { get; }

        public event EventHandler IsVisibleChangedEvent;


        public RowHeaderGroup(Department department, RowHeaderGroup parent)
        {
            Name = department.Name;
            SubName = string.Empty;
            Order = department.Order;
            IsRoot = department.IsRoot;
            Parent = parent;
            Level = IsRoot ? -1 : parent.Level + 1;
            Color = (Color)ColorConverter.ConvertFromString(department.HexColor);
            CanCollapse = (department.Childs?.Any() ?? false) || (department.Employees?.Any() ?? false);
            IsCollapsed = CanCollapse ? false : (bool?)null;
            IsVisible = true;

            Department = department;

            Childs = new List<RowHeaderGroup>();

            SetHeaderItems(department.Parameters);

            SwitchCollapseCommand = new DelegateCommand(SwitchCollapseExecute, () => CanCollapse);
        }

        public RowHeaderGroup(Employee employee, RowHeaderGroup parent)
        {
            Name = employee.Medic.FullName;
            SubName = employee.Specialty.Name;
            Order = employee.Order;
            IsRoot = false;
            Parent = parent;
            Level = parent.Level + 1;
            Color = Parent.Childs.Count % 2 == 0 ? Colors.WhiteSmoke : Colors.Transparent;
            CanCollapse = false;
            IsCollapsed = null;
            IsVisible = true;

            Employee = employee;

            Childs = new List<RowHeaderGroup>();

            SetHeaderItems(employee.Parameters);

            SwitchCollapseCommand = new DelegateCommand(SwitchCollapseExecute, () => CanCollapse);
        }

        
        public static RowHeaderGroup CreateHeadersRecursive(RowHeaderGroup parent, Department department)
        {
            var header = new RowHeaderGroup(department, parent);

            if (department.Childs?.Any() ?? false)
                foreach (var child in department.Childs)
                    header.Childs.Add(CreateHeadersRecursive(header, child));
            else if (department.Employees?.Any() ?? false)
                foreach (var employee in department.Employees)
                    header.Childs.Add(new RowHeaderGroup(employee, header));

            return header;
        }

        private void SetHeaderItems(List<Parameter> parameters)
        {
            HeaderItems = new List<RowHeaderItem>();

            if (parameters?.Any() ?? false)
                foreach (var parameter in parameters)
                    HeaderItems.Add(new RowHeaderItem(this, parameter));
        }

        private void SwitchCollapseExecute()
        {
            IsCollapsed = !IsCollapsed;

            foreach (var child in Childs)
                child.IsVisible = !IsCollapsed.Value;
        }
    }
}
