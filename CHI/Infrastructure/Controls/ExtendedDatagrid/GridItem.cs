using Prism.Mvvm;
using System;
using System.Windows.Media;

namespace CHI.Infrastructure
{
    public class GridItem : BindableBase
    {
        double? value;
        bool isVisible = true;
        bool isEditable = false;
        Color color;
        HeaderItem rowHeader;
        HeaderItem columnHeader;

        public double? Value { get => value; set => SetProperty(ref this.value, value); }
        public bool IsVisible { get => isVisible; set => SetProperty(ref isVisible, value); }
        public bool IsEditable { get => isEditable; set => SetProperty(ref isEditable, value); }
        public Color Color { get => color; set => SetProperty(ref color, value); }

        public GridItem(HeaderItem rowHeader, HeaderItem columnHeader, bool isEditable)
        {
            this.rowHeader = rowHeader;
            this.columnHeader = columnHeader;
            IsEditable = isEditable;

            rowHeader.ColorChangedEvent += (s, e) => Color = rowHeader.Color;
            rowHeader.IsVisibleChangedEvent += OnHeaderVisibleChanged;
            columnHeader.IsVisibleChangedEvent += OnHeaderVisibleChanged;
        }

        private void OnHeaderVisibleChanged(object sender, EventArgs e)
        {
            IsVisible = columnHeader.IsVisible && rowHeader.IsVisible;
        }
    }
}
