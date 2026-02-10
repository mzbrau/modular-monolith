using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TicketSystem.Client.Wpf.Converters;

/// <summary>
/// Converts null/non-null values to Visibility.
/// Non-null converts to Visible, null converts to Collapsed.
/// </summary>
public class NotNullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("NotNullToVisibilityConverter does not support ConvertBack");
    }
}
