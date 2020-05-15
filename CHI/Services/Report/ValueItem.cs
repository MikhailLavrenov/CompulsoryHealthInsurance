using CHI.Models.ServiceAccounting;
using Prism.Mvvm;
using System;
using System.Linq;
using System.Windows.Media;

namespace CHI.Services.Report
{
    public class ValueItem : BindableBase
    {
        double? value;
        bool isVisible = true;
        bool isWritable = false;
        Color color;

        public RowHeaderItem RowHeader { get; set; }
        public ColumnHeaderItem ColumnHeader { get; set; }
        public double? Value { get => value; set => SetProperty(ref this.value, value); }
        public bool IsVisible { get => isVisible; set => SetProperty(ref isVisible, value); }
        public bool IsWritable { get => isWritable; set => SetProperty(ref isWritable, value); }
        public Color Color { get => color; set => SetProperty(ref color, value); }
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public object ValueContext { get; set; }

        public ValueItem(int rowIndex, int columnIndex, RowHeaderItem rowHeader, ColumnHeaderItem columnHeader, bool isReadOnly)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            RowHeader = rowHeader;
            ColumnHeader = columnHeader;
            Color = rowHeader.Group.Color;

            IsWritable = !isReadOnly && columnHeader.Group.Component.IsCanPlanning
                && (RowHeader.Parameter.Kind == ParameterKind.EmployeePlan || RowHeader.Parameter.Kind == ParameterKind.DepartmentHandPlan)
                && !(RowHeader.Group.Department?.Childs.Any() ?? false);

            ColumnHeader.Group.IsVisibleChangedEvent += OnHeaderGroupVisibleChanged;
            RowHeader.Group.IsVisibleChangedEvent += OnHeaderGroupVisibleChanged;
            RowHeader.Group.ColorChangedEvent += OnHeaderColorChanged;
        }

        private void OnHeaderGroupVisibleChanged(object sender, EventArgs e)
        {
            IsVisible = ColumnHeader.Group.IsVisible && RowHeader.Group.IsVisible;
        }

        private void OnHeaderColorChanged(object sender, EventArgs e)
        {
            Color = RowHeader.Group.Color;
        }
    }
}
