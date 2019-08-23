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

            // Когда DataGridTemplateColumn переводится в режим редактирования вручную, 
            // чтобы не делать лишний клик, нужно вручную установить фокус на элемент управления
            if (e.EditingElement.GetType().Name == "ContentPresenter")
            {
                var control = FindVisualChild<Control>(e.EditingElement);
                if (control != null && control.IsFocused == false)
                    control.Focus();
            }

            //Исключает выделение текста при выборе ячейки TextBox
            if (e.EditingElement is TextBox textBox)
                if (textBox.SelectionLength != 0)
                    textBox.CaretIndex = textBox.Text.Length;
        }
        // Возникает при любом клике ЛКМ по DataGrid'у
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            var mbe = e as MouseButtonEventArgs;
            var clickedElement = mbe.OriginalSource as UIElement;

            // Исключает баг исчезновения пароля в PasswordBox  
            var originalSourceName = mbe.OriginalSource.GetType().Name;

            if (originalSourceName == "Border" || originalSourceName == "DataGridCell")
                if (FindVisualChild<PasswordBox>(clickedElement) != null)
                    return;

            // Далее редактирование ячеек одним кликом
            var cell = FindVisualParent<DataGridCell>(clickedElement);

            if (cell == null || cell.IsEditing || cell.IsReadOnly)
                return;

            // 1. Нужно установить фокус на выбранной ячейке
            if (cell.IsFocused == false)
                cell.Focus();

            // 2. Нужно установить IsSelected на строку или ячейку в зависимости от стиля выделения курсора в DataGrid
            // Проверка обязательна, тупо выделять ячейку нельзя, возникнет исключение
            if (SelectionUnit == DataGridSelectionUnit.FullRow)
            {
                var row = FindVisualParent<DataGridRow>(cell);

                if (row?.IsSelected == false)
                    row.IsSelected = true;
            }
            else if (cell.IsSelected == false)
                cell.IsSelected = true;

            // Исключает баг. Если добавление новой строки начинается c DataGridTemplateColumn, новый элемент коллекции не получит  
            // нужный тип пока текущая ячейка не будет переведена в режим редактрования вручную. В противном случае могут появиться  
            // лишние строки и новая строка не попадет в связанную пользовательскую коллекцию
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
        // Рекурсивно находит дочерний элемент соответствующий типу T 
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
