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
        HeaderSubItem rowSubHeader;
        HeaderSubItem columnSubHeader;

        public double? Value { get => value; set => SetProperty(ref this.value, value); }
        public bool IsVisible { get => isVisible; set => SetProperty(ref isVisible, value); }
        public bool IsEditable { get => isEditable; set => SetProperty(ref isEditable, value); }
        public Color Color { get => color; set => SetProperty(ref color, value); }

        public GridItem(HeaderSubItem rowSubHeader, HeaderSubItem columnSubHeader, bool isEditable)
        {
            this.rowSubHeader = rowSubHeader;
            this.columnSubHeader = columnSubHeader;
            Color = rowSubHeader.HeaderItem.Color;
            IsEditable = isEditable;

            rowSubHeader.HeaderItem.ColorChangedEvent += (s, e) => Color = rowSubHeader.HeaderItem.Color;
            rowSubHeader.HeaderItem.IsVisibleChangedEvent += OnHeaderVisibleChanged;
            columnSubHeader.HeaderItem.IsVisibleChangedEvent += OnHeaderVisibleChanged;
        }

        private void OnHeaderVisibleChanged(object sender, EventArgs e)
        {
            IsVisible = columnSubHeader.HeaderItem.IsVisible && rowSubHeader.HeaderItem.IsVisible;
        }
    }
}
