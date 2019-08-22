using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PatientsFomsRepository.Infrastructure
{
    /// <summary>
    /// DataGrid с настроенным поведением при редактировании ячеек
    /// </summary>
    public class CustomDataGrid : DataGrid
    {
        //Исключает выделение текста при выборе новой ячейки
        protected override void OnPreparingCellForEdit(DataGridPreparingCellForEditEventArgs e)
        {
            base.OnPreparingCellForEdit(e);

            var textBox = e.EditingElement as TextBox;

            if (textBox != null && textBox.SelectionLength != 0)
                textBox.CaretIndex = textBox.Text.Length;
        }
        //Редактирование ячеек одним кликом
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            var mbe = e as MouseButtonEventArgs;
            var clickedElement = mbe.OriginalSource as UIElement;

            //исключает баг исчезновения пароля в PasswordBox  
            var originalSourceName = mbe.OriginalSource.GetType().Name;
            if (originalSourceName == "Border" || originalSourceName == "DataGridCell")
                if (FindVisualChild<PasswordBox>(clickedElement) != null)
                    return;

            var cell = FindVisualParent<DataGridCell>(clickedElement);

            if (cell == null || cell.IsEditing || cell.IsReadOnly)
                return;

            if (cell.IsFocused == false)
                cell.Focus();


            var dataGrid = FindVisualParent<DataGrid>(cell);

            if (dataGrid == null)
                return;

            if (dataGrid.SelectionUnit == DataGridSelectionUnit.FullRow)
            {
                var row = FindVisualParent<DataGridRow>(cell);

                if (row?.IsSelected == false)
                    row.IsSelected = true;
            }

            else if (cell.IsSelected == false)
                cell.IsSelected = true;
        }
        //Находит родительский элемент соответствующий типу T 
        private static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            while (element != null)
            {
                if (element is T correctlyTyped)
                    return correctlyTyped;

                element = VisualTreeHelper.GetParent(element) as UIElement;
            }

            return null;
        }
        //Рекурсивно находит дочерний элемент соответствующий типу T 
        public static T FindVisualChild<T>(UIElement element) where T : UIElement
        {
            if (element == null)
                return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(element);

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i) as UIElement;

                if (child is T correctlyTyped)
                    return correctlyTyped;

                var result = FindVisualChild<T>(child);

                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
