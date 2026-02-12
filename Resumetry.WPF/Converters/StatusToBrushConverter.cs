using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Resumetry.Domain.Enums;

namespace Resumetry.Converters
{
    /// <summary>
    /// Converts StatusEnum values to colored brushes for UI display.
    /// </summary>
    public class StatusToBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush AppliedBrush = CreateFrozenBrush(33, 150, 243); // Blue
        private static readonly SolidColorBrush RejectedBrush = CreateFrozenBrush(244, 67, 54); // Red
        private static readonly SolidColorBrush ScreenBrush = CreateFrozenBrush(255, 152, 0); // Orange
        private static readonly SolidColorBrush InterviewBrush = CreateFrozenBrush(156, 39, 176); // Purple
        private static readonly SolidColorBrush OfferBrush = CreateFrozenBrush(76, 175, 80); // Green
        private static readonly SolidColorBrush WithdrawnBrush = CreateFrozenBrush(158, 158, 158); // Gray
        private static readonly SolidColorBrush NoOfferBrush = CreateFrozenBrush(255, 87, 34); // Deep Orange
        private static readonly SolidColorBrush DefaultBrush = CreateFrozenBrush(158, 158, 158); // Gray

        private static SolidColorBrush CreateFrozenBrush(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze(); // Make immutable and thread-safe
            return brush;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StatusEnum status)
            {
                return status switch
                {
                    StatusEnum.Applied => AppliedBrush,
                    StatusEnum.Rejected => RejectedBrush,
                    StatusEnum.Screen => ScreenBrush,
                    StatusEnum.Interview => InterviewBrush,
                    StatusEnum.Offer => OfferBrush,
                    StatusEnum.Withdrawn => WithdrawnBrush,
                    StatusEnum.NoOffer => NoOfferBrush,
                    _ => DefaultBrush
                };
            }
            return DefaultBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
