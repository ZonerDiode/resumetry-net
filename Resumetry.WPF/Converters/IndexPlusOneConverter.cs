using System;
using System.Globalization;
using System.Windows.Data;

namespace Resumetry.Converters
{
    public class IndexPlusOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int idx) return (idx + 1).ToString();
            if (int.TryParse(value?.ToString(), out var i)) return (i + 1).ToString();
            return value ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}