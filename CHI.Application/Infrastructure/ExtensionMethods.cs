using CHI.Application.Views;
using Prism.Services.Dialogs;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace CHI.Application.Infrastructure
{
    /// <summary>
    /// Содержит все расширяющие методы
    /// </summary>
    public static class ExtensionMethods
    {
        // Находит в логическом дереве родительский элемент соответствующий типу T
        public static T FindLogicalParent<T>(this UIElement element) where T : UIElement
        {
            while (element != null)
            {
                if (element is T correctlyTyped)
                    return correctlyTyped;

                element = LogicalTreeHelper.GetParent(element) as UIElement;
            }

            return null;
        }
        // Находит в визульном дереве родительский элемент соответствующий типу T 
        public static T FindVisualParent<T>(this UIElement element) where T : UIElement
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
        public static T FindVisualVisibleChild<T>(this UIElement element) where T : UIElement
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
        /// <summary>
        /// Вызывает диалоговое окно модально
        /// </summary>
        public static ButtonResult ShowDialog(this IDialogService dialogService, string title, string message)
        {
            IDialogResult result = null;
            var dialogName = nameof(NotificationDialogView);
            var dialogParameters = new DialogParameters();
            dialogParameters.Add("title", title);
            dialogParameters.Add("message", message);
            var dispatcher = System.Windows.Application.Current.Dispatcher;

            var showDialog = (Action)(() => dialogService.ShowDialog(dialogName, dialogParameters, x => result = x));

            dispatcher.BeginInvoke(showDialog).Task.GetAwaiter().GetResult();

            return result.Result;
        }
        /// <summary>
        /// Возвращает массив байтов потока
        /// </summary>
        /// <param name="stream">Поток</param>
        /// <returns>Массив байтов</returns>
        public static byte[] GetBytes(this Stream stream)
        {
            stream.Position = 0;

            byte[] result;

            using (var mStream = new MemoryStream())
            {
                stream.CopyTo(mStream);
                result = mStream.ToArray();
            }

            return result;
        }
    }
}
