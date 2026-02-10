using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TicketSystem.Client.Wpf.Converters;

/// <summary>
/// Converts null/non-null values to Visibility.
/// Null converts to Visible, non-null converts to Collapsed.
/// Useful for showing placeholder text when no item is selected.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("NullToVisibilityConverter does not support ConvertBack");
    }
}
