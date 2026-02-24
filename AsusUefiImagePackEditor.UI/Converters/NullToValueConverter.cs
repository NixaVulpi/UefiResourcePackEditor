using System;
using System.Globalization;
using System.Windows.Data;

namespace AsusUefiImagePackEditor.UI.Converters
{
    public class NullToValueConverter: IValueConverter
    {
        public object? NullValue { get; set; }
        public object? NotNullValue { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is null ? NullValue : NotNullValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
