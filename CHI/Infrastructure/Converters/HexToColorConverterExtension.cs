using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace CHI.Infrastructure
{
    [ValueConversion(typeof(string), typeof(Color))]
    public class HexToColorConverterExtension : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hex = (string)value;

            if (string.IsNullOrEmpty(hex) || hex.Length != 7 || hex[0] != '#')
                return  Colors.White;

            return (Color)ColorConverter.ConvertFromString(hex);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var drawingColor = Helpers.GetDrawingColor((Color)value);

            return System.Drawing.ColorTranslator.ToHtml(drawingColor);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

