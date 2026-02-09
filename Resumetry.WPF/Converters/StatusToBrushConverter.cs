using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Resumetry.Domain.Enums;

namespace Resumetry.Converters
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StatusEnum status)
            {
                return status switch
                {
                    StatusEnum.APPLIED => new SolidColorBrush(Color.FromRgb(33, 150, 243)), // Blue
                    StatusEnum.REJECTED => new SolidColorBrush(Color.FromRgb(244, 67, 54)), // Red
                    StatusEnum.SCREEN => new SolidColorBrush(Color.FromRgb(255, 152, 0)), // Orange
                    StatusEnum.INTERVIEW => new SolidColorBrush(Color.FromRgb(156, 39, 176)), // Purple
                    StatusEnum.OFFER => new SolidColorBrush(Color.FromRgb(76, 175, 80)), // Green
                    StatusEnum.WITHDRAWN => new SolidColorBrush(Color.FromRgb(158, 158, 158)), // Gray
                    StatusEnum.NOOFFER => new SolidColorBrush(Color.FromRgb(255, 87, 34)), // Deep Orange
                    _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))
                };
            }
            return new SolidColorBrush(Color.FromRgb(158, 158, 158));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
