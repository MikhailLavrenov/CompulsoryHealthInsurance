using CHI.Infrastructure.Controls._2DDataGrid;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CHI.Infrastructure.Controls
{
    /// <summary>
    /// Логика взаимодействия для Hierarchical2DDatagrid.xaml
    /// </summary>
    public partial class ExtendedDatagrid : UserControl
    {
        public ExtendedDatagrid()
        {
            InitializeComponent();
        }


        public List<HeaderItem> RowHeaderItems
        {
            get => (List<HeaderItem>)GetValue(RowHeaderItemsProperty);
            set => SetValue(RowHeaderItemsProperty, value);
        }

        public static readonly DependencyProperty RowHeaderItemsProperty =
            DependencyProperty.Register(nameof(RowHeaderItems), typeof(List<HeaderItem>), typeof(ExtendedDatagrid));

        
        public List<HeaderItem> ColumnHeaderItems
        {
            get => (List<HeaderItem>)GetValue(ColumnHeaderItemsProperty); 
            set => SetValue(ColumnHeaderItemsProperty, value); 
        }

        public static readonly DependencyProperty ColumnHeaderItemsProperty =
            DependencyProperty.Register(nameof(ColumnHeaderItems), typeof(List<HeaderItem>), typeof(ExtendedDatagrid));


        public HeaderItem[][] GridItems
        {
            get => (HeaderItem[][])GetValue(GridItemsProperty);
            set => SetValue(GridItemsProperty, value);
        }

        public static readonly DependencyProperty GridItemsProperty =
            DependencyProperty.Register(nameof(ColumnHeaderItems), typeof(HeaderItem[][]), typeof(ExtendedDatagrid));





    }
}
