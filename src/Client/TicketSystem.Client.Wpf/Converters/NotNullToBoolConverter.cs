using System.Globalization;
using System.Windows.Data;

namespace TicketSystem.Client.Wpf.Converters;

/// <summary>
/// Converts null/non-null values to boolean.
/// Returns true if value is not null, false if value is null.
/// </summary>
public class NotNullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("NotNullToBoolConverter does not support ConvertBack");
    }
}
