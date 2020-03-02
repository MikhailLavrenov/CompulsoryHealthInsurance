using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CHI.Infrastructure
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
                var control = e.EditingElement.FindVisualVisibleChild<Control>();

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
            var cell = clickedElement.FindVisualParent<DataGridCell>();

            if (cell == null || cell.IsEditing || cell.IsReadOnly)
                return;

            if (cell.IsFocused == false)
                cell.Focus();

            // Условие обязательно, тупо выделять ячейку нельзя, может возникнуть исключение
            if (SelectionUnit == DataGridSelectionUnit.FullRow)
            {
                var row = cell.FindVisualParent<DataGridRow>();

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
    }
}
