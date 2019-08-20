using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PatientsFomsRepository.Infrastructure
{
    //Для редактирования ячеек datagrid одним кликом
    public static class DataGridHelper
    {
        internal static void DataGridPreviewLeftMouseButtonDownEvent(object sender, RoutedEventArgs e)
        {
            var mbe = e as MouseButtonEventArgs;

            //исключает баг исчезновения пароля в PasswordBox
            var originalSourceName = mbe.OriginalSource.GetType().Name;
            if (originalSourceName == "Border" || originalSourceName == "DataGridCell")
                return;

            var element = mbe.OriginalSource as UIElement;
            var cell = element.FindVisualParent<DataGridCell>();

            if (cell == null || cell.IsEditing || cell.IsReadOnly)
                return;

            if (cell.IsFocused == false)
                cell.Focus();

            var dataGrid = cell.FindVisualParent<DataGrid>();

            if (dataGrid == null)
                return;

            if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
            {
                if (cell.IsSelected == false)
                    cell.IsSelected = true;
            }
            else
            {
                var row = cell.FindVisualParent<DataGridRow>();

                if (row?.IsSelected == false)
                    row.IsSelected = true;
            }
        }
        private static T FindVisualParent<T>(this UIElement element) where T : UIElement
        {
            while (element != null)
            {
                if (element is T correctlyTyped)
                    return correctlyTyped;

                element = VisualTreeHelper.GetParent(element) as UIElement;
            }

            return null;
        }
    }
}
