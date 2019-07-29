﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace PatientsFomsRepository.Infrastructure
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibleConverterExtension : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visible = (bool)value;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visible = (Visibility)value;
            return visible == Visibility.Visible ? true : false;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
