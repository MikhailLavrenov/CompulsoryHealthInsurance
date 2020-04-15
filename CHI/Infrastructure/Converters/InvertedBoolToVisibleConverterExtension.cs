using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace CHI.Infrastructure
{
    /// <summary>
    /// Расширение разметки xaml, конвертирует !bool в Visibility
    /// </summary>
    [ValueConversion(typeof(bool?), typeof(Visibility))]
    public class InvertedBoolToVisibleConverterExtension : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visible = !(bool?)value;
            return visible == true ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visible = (Visibility)value;
            return visible == Visibility.Visible ? false : true;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
