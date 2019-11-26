using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace CHI.Application.Infrastructure
{
    /// <summary>
    /// Расширение разметки xaml, конвертирует bool в Visibility
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InvertBoolConverterExtension : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}