using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Windows.Media;

namespace CHI.Infrastructure
{
    public class GridItem : BindableBase
    {
        double? value;
        bool isVisible = true;
        bool isEditable = false;
        Color color;
        
        public HeaderSubItem RowSubHeader { get; }
        public HeaderSubItem ColumnSubHeader { get; }
        public double? Value { get => value; set => SetProperty(ref this.value, value); }
        public bool IsVisible { get => isVisible; set => SetProperty(ref isVisible, value); }
        public bool IsEditable { get => isEditable; set => SetProperty(ref isEditable, value); }
        public Color Color { get => color; set => SetProperty(ref color, value); }

        public GridItem(HeaderSubItem rowSubHeader, HeaderSubItem columnSubHeader)
        {
            RowSubHeader = rowSubHeader;
            ColumnSubHeader = columnSubHeader;
            Color = rowSubHeader.HeaderItem.Color;

            rowSubHeader.HeaderItem.PropertyChanged += OnRowPropertyChanged;
            columnSubHeader.HeaderItem.PropertyChanged += OnColumnPropertyChanged;
        }

        private void OnColumnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(HeaderItem.IsVisible))
                IsVisible = ColumnSubHeader.HeaderItem.IsVisible && RowSubHeader.HeaderItem.IsVisible;
        }
        private void OnRowPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName==nameof(HeaderItem.Color))
                Color = RowSubHeader.HeaderItem.Color;
            else if (args.PropertyName == nameof(HeaderItem.IsVisible))
                IsVisible = ColumnSubHeader.HeaderItem.IsVisible && RowSubHeader.HeaderItem.IsVisible;
        }
    }
}
