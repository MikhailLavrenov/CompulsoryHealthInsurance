using Prism.Mvvm;
using System.Windows.Media;

namespace CHI.Services.Report
{
    public class ValueItem : BindableBase
    {
        double? value;
        bool isVisible;

        public RowHeaderItem RowHeader { get; set; }
        public ColumnHeaderItem ColumnHeader { get; set; }
        public double? Value { get => value; set => SetProperty(ref this.value, value); }
        public bool IsVisible { get => isVisible; set => SetProperty(ref isVisible, value); }
        public Color Color { get; set; }
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public object ValueContext { get; set; }

        public ValueItem(int rowIndex, int columnIndex, RowHeaderItem rowHeader, ColumnHeaderItem columnHeader)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            RowHeader = rowHeader;
            ColumnHeader = columnHeader;
            IsVisible = true;
            Color = rowHeader.Group.Color;
            
            ColumnHeader.Group.IsVisibleChangedEvent += OnHeaderGroupVisibleChanged;
            RowHeader.Group.IsVisibleChangedEvent += OnHeaderGroupVisibleChanged;
        }

        private void OnHeaderGroupVisibleChanged(object sender, System.EventArgs e)
        {
            IsVisible = ColumnHeader.Group.IsVisible && RowHeader.Group.IsVisible;
        }
    }
}
