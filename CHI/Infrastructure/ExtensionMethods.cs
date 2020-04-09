using CHI.Models.Infrastructure;
using CHI.Models.ServiceAccounting;
using CHI.Views;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace CHI.Infrastructure
{
    /// <summary>
    /// Содержит все расширяющие методы
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Возвращает описание (Description) перечисления (enum)
        /// </summary>
        /// <param name="value">Перечисление (enum)</param>
        /// <returns>Описание</returns>
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            Attribute attribute = Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute), false);
            DescriptionAttribute descriptionAttribute = attribute as DescriptionAttribute;

            return descriptionAttribute.Description;
        }
        /// <summary>
        /// Возвращает список всех значений enum и их описаний 
        /// </summary>
        /// <typeparam name="T">Перечисление (enum)</typeparam>
        /// <returns>Перечисление значений и описаний</returns>
        public static IEnumerable<KeyValuePair<Enum, string>> GetAllValuesAndDescriptions(this Enum en)
        {
            return Enum.GetValues(en.GetType())
                .Cast<Enum>()
                .Select(x => new KeyValuePair<Enum, string>(x, x.GetDescription()))
                .ToList();
        }
        // Находит в логическом дереве родительский элемент с заданным Именем
        public static FrameworkElement FindParent(this FrameworkElement element, string foundName)
        {
            while (element != null)
            {
                if (element.Name==foundName)
                    return element;

                element = element.Parent as FrameworkElement;
            }

            return null;
        }
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

        public static List<T> ToListRecursive<T>(this T obj) where T: class, IOrderedHierarchical<T>
        {
            var result = new List<T>();

            obj.ToListRecursive(result);

            return result;
        }

        public static void ToListRecursive<T>(this T obj, List<T> result) where T : class, IOrderedHierarchical<T>
        {
            result.Add(obj);

            if (obj.Childs != null)
                foreach (var child in obj.Childs)
                    child.ToListRecursive(result);
        }

        public static void OrderChildsRecursive<T>(this T obj) where T : class, IOrderedHierarchical<T>
        {
            if (obj.Childs == null)
                return;

            obj.Childs = obj.Childs.OrderBy(x => x.Order).ToList();

            foreach (var child in obj.Childs)
                child.OrderChildsRecursive();
        }

        public static bool PeriodBetweenDates(DateTime? date1, DateTime? date2, int periodMonth, int periodYear)
        {
            var limit1 = date1.HasValue ? date1.Value.Year * 100 + date1.Value.Month : 0;
            var limit2 = date2.HasValue ? date2.Value.Year * 100 + date2.Value.Month : int.MaxValue;
            var period = periodYear * 100 + periodMonth;

            return limit1 <= period && period <= limit2;
        }

    }
}
