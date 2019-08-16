using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PatientsFomsRepository.Infrastructure
{
    //Для редактирования ячеек datagrid одним кликом
    public static class DataGridHelper
    {
        internal static void DataGridPreviewMouseLeftButtonDownEvent(object sender, System.Windows.RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            var mbe = e as MouseButtonEventArgs;

            DependencyObject obj = null;
            if (mbe != null)
            {
                obj = mbe.OriginalSource as DependencyObject;
                while (obj != null && !(obj is DataGridCell))
                {
                    obj = VisualTreeHelper.GetParent(obj);
                }
            }

            DataGridCell cell = null;
            DataGrid dataGrid = null;

            if (obj != null)
                cell = obj as DataGridCell;

            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                if (!cell.IsFocused)
                {
                    cell.Focus();
                }
                dataGrid = FindVisualParent<DataGrid>(cell);
                if (dataGrid != null)
                {
                    if (dataGrid.SelectionUnit
                        != DataGridSelectionUnit.FullRow)
                    {
                        if (!cell.IsSelected)
                            cell.IsSelected = true;
                    }
                    else
                    {
                        var row = FindVisualParent<DataGridRow>(cell);
                        if (row != null && !row.IsSelected)
                        {
                            row.IsSelected = true;
                        }
                    }
                }
            }

        }
        static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }
    }
}
