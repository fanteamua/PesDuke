using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PesDuke.Converters;

public class BoolToSpeakingBgConverter : IValueConverter
{
    private static readonly SolidColorBrush AccentBrush = new(Color.FromRgb(0, 87, 183));
    private static readonly SolidColorBrush TransparentBrush = Brushes.Transparent;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? AccentBrush : TransparentBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
