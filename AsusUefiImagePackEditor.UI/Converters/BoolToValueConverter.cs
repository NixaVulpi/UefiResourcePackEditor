using System;
using System.Globalization;
using System.Windows.Data;

namespace AsusUefiImagePackEditor.UI.Converters
{
    public class BoolToValueConverter: IValueConverter
    {
        public object? TrueValue { get; set; }
        public object? FalseValue { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value as bool? == true ? TrueValue : FalseValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
