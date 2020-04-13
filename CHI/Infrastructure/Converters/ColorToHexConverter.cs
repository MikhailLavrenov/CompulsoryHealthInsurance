using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace CHI.Infrastructure
{
    [ValueConversion(typeof(string), typeof(Color))]
    public class HexToSolidColorBrushConverterExtension : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hex = (string)value;

            if (string.IsNullOrEmpty(hex) || hex.Length != 7 || hex[0] != '#')
                return new SolidColorBrush(Colors.White);

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var drawingColor = ExtensionMethods.GetDrawingColor(((SolidColorBrush)value).Color);

            return System.Drawing.ColorTranslator.ToHtml(drawingColor);
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

