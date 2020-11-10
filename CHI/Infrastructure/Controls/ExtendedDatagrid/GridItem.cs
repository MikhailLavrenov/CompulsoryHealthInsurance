using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace CHI.Infrastructure.Controls._2DDataGrid
{
    public class GridItem : BindableBase
    {
        double? value;
        bool isVisible = true;
        bool isWritable = false;
        Color color;

        public double? Value { get => value; set => SetProperty(ref this.value, value); }
        public bool IsVisible { get => isVisible; set => SetProperty(ref isVisible, value); }
        public bool IsWritable { get => isWritable; set => SetProperty(ref isWritable, value); }
        public Color Color { get => color; set => SetProperty(ref color, value); }
    }
}
