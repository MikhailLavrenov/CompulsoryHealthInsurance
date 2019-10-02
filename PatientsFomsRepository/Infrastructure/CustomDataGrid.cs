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
        // Возникает перед редактированием ячейки
        protected override void OnPreparingCellForEdit(DataGridPreparingCellForEditEventArgs e)
        {
            base.OnPreparingCellForEdit(e);

            // Если редактируется ячейка DataGridTemplateColumn
            if (e.Column.GetType() == typeof(DataGridTemplateColumn))
            {
                var control = FindVisualVisibleChild<Control>(e.EditingElement);

                // Установливает фокус на элемент управления, чтобы исключить лишний клик
                if (control?.IsFocused == false)
                    control.Focus();
            }

            //Исключает выделение текста при выборе стандартной ячейки DataGridTextColumn
            else if (e.Column.GetType() == typeof(DataGridTextColumn))
            {
                var textBox = (TextBox)e.EditingElement;
                if (textBox.SelectionLength != 0)
                    textBox.CaretIndex = textBox.Text.Length;
            }
        }
        // Редактирование стандартных ячеек одним кликом (устанавливает фокус на ячейку и IsSelected)
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            var mbe = e as MouseButtonEventArgs;
            var clickedElement = mbe.OriginalSource as UIElement;
            var cell = FindVisualParent<DataGridCell>(clickedElement);

            if (cell == null || cell.IsEditing || cell.IsReadOnly)
                return;

            if (cell.IsFocused == false)
                cell.Focus();

            // Условие обязательно, тупо выделять ячейку нельзя, может возникнуть исключение
            if (SelectionUnit == DataGridSelectionUnit.FullRow)
            {
                var row = FindVisualParent<DataGridRow>(cell);

                if (row?.IsSelected == false)
                    row.IsSelected = true;
            }
            else if (cell.IsSelected == false)
                cell.IsSelected = true;

            // При добавлении новой строки через DataGridTemplateColumn - ячейку нужно переводить в режим редактирования вручную, 
            // иначе не создастся экземпляр коллекции элементов DataGrid
            var currentItemName = CurrentItem?.GetType().Name;

            if (currentItemName == "NamedObject")
                BeginEdit();
        }
        // Находит родительский элемент соответствующий типу T 
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
        // Рекурсивно находит дочерний видимый элемент соответствующий типу T 
        public static T FindVisualVisibleChild<T>(UIElement element) where T : UIElement
        {
            if (element == null)
                return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(element);

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i) as UIElement;

                if (child is T correctlyTyped)
                    if (correctlyTyped.IsVisible)
                        return correctlyTyped;

                var result = FindVisualVisibleChild<T>(child);

                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
