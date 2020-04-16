using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace CHI.Infrastructure
{
    //Расширение разметки xaml, конвертирует  enum в коллекцию всех значений Description
    [ValueConversion(typeof(Enum), typeof(IEnumerable<KeyValuePair<Enum, string>>))]
    public class EnumToCollectionConverterExtension : MarkupExtension,IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Helpers.GetAllValuesAndDescriptions(value.GetType());
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
