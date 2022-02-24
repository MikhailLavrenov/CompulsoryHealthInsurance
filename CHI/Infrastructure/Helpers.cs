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
    public static class Helpers
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

            return descriptionAttribute?.Description ?? string.Empty; ;
        }

        /// <summary>
        /// Возвращает  краткое описание (ShortDescription) перечисления (enum)
        /// </summary>
        /// <param name="value">Перечисление (enum)</param>
        /// <returns>Краткое описание</returns>
        public static string GetShortDescription(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            Attribute attribute = Attribute.GetCustomAttribute(fieldInfo, typeof(MultipleDescriptionAttribute), false);
            MultipleDescriptionAttribute descriptionAttribute = attribute as MultipleDescriptionAttribute;

            return descriptionAttribute?.ShortDescription ?? string.Empty;
        }

        /// <summary>
        /// Возвращает список всех значений enum и их описаний 
        /// </summary>
        /// <typeparam name="T">Перечисление (enum)</typeparam>
        /// <returns>Перечисление значений и описаний</returns>
        public static List<KeyValuePair<Enum, string>> GetAllValuesAndDescriptions(Type enumType)
        {
            if (!enumType.IsSubclassOf(typeof(Enum)))
                throw new ArgumentException("Предоставленный тип не является наследником Enum");

            return Enum.GetValues(enumType)
                .Cast<Enum>()
                .Select(x => new KeyValuePair<Enum, string>(x, x.GetDescription()))
                .ToList();
        }

        /// <summary>
        /// Возвращает список всех значений enum и их кратких описаний 
        /// </summary>
        /// <typeparam name="T">Перечисление (enum)</typeparam>
        /// <returns>Перечисление значений и кратких описаний</returns>
        public static List<KeyValuePair<Enum, string>> GetAllValuesAndShortDescriptions(Type enumType)
        {
            if (!enumType.IsSubclassOf(typeof(Enum)))
                throw new ArgumentException("Предоставленный тип не является наследником Enum");

            return Enum.GetValues(enumType)
                .Cast<Enum>()
                .Select(x => new KeyValuePair<Enum, string>(x, x.GetShortDescription()))
                .ToList();
        }

        // Находит в логическом дереве родительский элемент с заданным Именем
        public static FrameworkElement FindParent(this FrameworkElement element, string foundName)
        {
            while (element != null)
            {
                if (element.Name == foundName)
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
        public static T FindVisualParent<T>(this Visual element) where T : UIElement
        {
            while (element != null)
            {
                if (element is T correctlyTyped)
                    return correctlyTyped;

                element = VisualTreeHelper.GetParent(element) as Visual;
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

        ///// <summary>
        ///// Вызывает текстовое диалоговое окно модально
        ///// </summary>
        //public static ButtonResult ShowTextDialog(this IDialogService dialogService, string title, string message)
        //{
        //    IDialogResult result = null;
        //    var dialogName = nameof(NotificationDialogView);
        //    var dialogParameters = new DialogParameters();
        //    dialogParameters.Add("title", title);
        //    dialogParameters.Add("message", message);

        //    Application.Current.Dispatcher.Invoke(() => dialogService.ShowDialog(dialogName, dialogParameters, x => result = x));

        //    return result.Result;
        //}

        ///// <summary>
        ///// Вызывает диалоговое окно выбора цвета модально
        ///// </summary>
        //public static string ShowColorDialog(this IDialogService dialogService, string title, string hexColor)
        //{
        //    IDialogResult result = null;
        //    var dialogName = nameof(ColorDialogView);

        //    var color = (Color)ColorConverter.ConvertFromString(hexColor);
        //    var dialogParameters = new DialogParameters();
        //    dialogParameters.Add("title", title);
        //    dialogParameters.Add("color", color);

        //    Application.Current.Dispatcher.Invoke(() => dialogService.ShowDialog(dialogName, dialogParameters, x => result = x));

        //    if (result.Result == ButtonResult.OK)
        //    {
        //        color = result.Parameters.GetValue<Color>("color");

        //        return System.Drawing.ColorTranslator.ToHtml(GetDrawingColor(color));
        //    }
        //    else
        //        return hexColor;
        //}

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

        public static List<T> ToListRecursive<T>(this T obj) where T : class, IHierarchical<T>
        {
            var result = new List<T>();

            obj.ToListRecursive(result);

            return result;
        }

        static void ToListRecursive<T>(this T obj, List<T> result) where T : class, IHierarchical<T>
        {
            result.Add(obj);

            if (obj.Childs != null)
                foreach (var child in obj.Childs)
                    child.ToListRecursive(result);
        }

        public static void OrderChildsRecursive<T>(this T obj) where T : class, IHierarchical<T>, IOrdered
        {
            if (obj.Childs == null)
                return;

            obj.Childs = obj.Childs.OrderBy(x => x.Order).ToList();

            foreach (var child in obj.Childs)
                child.OrderChildsRecursive();
        }

        public static bool BetweenDates(DateTime? date1, DateTime? date2, int periodMonth, int periodYear)
        {
            var limit1 = date1.HasValue ? date1.Value.Year * 100 + date1.Value.Month : 0;
            var limit2 = date2.HasValue ? date2.Value.Year * 100 + date2.Value.Month : int.MaxValue;
            var period = periodYear * 100 + periodMonth;

            return limit1 <= period && period <= limit2;
        }

        public static System.Drawing.Color GetDrawingColor(System.Windows.Media.Color mediaColor)
        {
            return System.Drawing.Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        }

        public static bool IsFileLocked(string path)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
                stream.Close();
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Возвращает подстроку между левой leftStr и правой rightStr строками, поиск начинается от начальной позиции offsetStr.
        /// Если одна из строк не найдена-возвращает пустую строку.
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="offsetStr">Подстрока, после которой начинается поиск. Можеть быть null или пустой</param>
        /// <param name="leftStr">Левая подстрока</param>
        /// <param name="rightStr">Правая подстрока</param>
        /// <returns>Искомая подстрока, иначе пустая</returns>
        public static string SubstringBetween(this string text, string offsetStr, string leftStr, string rightStr)
        {
            int offset;

            if (string.IsNullOrEmpty(offsetStr))
                offset = 0;
            else
            {
                offset = text.IndexOf(offsetStr);

                if (offset == -1)
                    return string.Empty;

                offset += offsetStr.Length;
            }

            var begin = text.IndexOf(leftStr, offset);

            if (begin == -1)
                return string.Empty;

            begin += leftStr.Length;

            var end = text.IndexOf(rightStr, begin);

            if (end == -1)
                return string.Empty;

            var length = end - begin;

            if (length > 0)
                return text.Substring(begin, length);

            return string.Empty;
        }
    }
}
