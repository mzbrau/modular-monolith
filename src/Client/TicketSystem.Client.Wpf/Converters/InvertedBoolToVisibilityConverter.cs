using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TicketSystem.Client.Wpf.Converters;

/// <summary>
/// Converts boolean values to Visibility enum values with inverted logic.
/// False converts to Visible, True converts to Collapsed.
/// </summary>
public class InvertedBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility != Visibility.Visible;
        }

        return true;
    }
}
