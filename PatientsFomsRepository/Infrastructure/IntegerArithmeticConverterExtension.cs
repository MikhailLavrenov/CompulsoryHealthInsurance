using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace PatientsFomsRepository.Infrastructure
{
    /// <summary>
    /// Расширение разметки xaml, конвертирует bool в Visibility
    /// </summary>
    [ValueConversion(typeof(int?), typeof(double))]
    public class IntegerMultiplyConverterExtension : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            var a = int.Parse(value.ToString(), CultureInfo.InvariantCulture);
            var b = double.Parse(parameter.ToString(), CultureInfo.InvariantCulture);


            var result = (double?)(a * b);
            return (double)(-300);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}