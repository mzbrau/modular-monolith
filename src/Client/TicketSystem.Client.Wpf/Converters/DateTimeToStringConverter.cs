using System.Globalization;
using System.Windows.Data;

namespace TicketSystem.Client.Wpf.Converters;

/// <summary>
/// Converts DateTime values to formatted display strings.
/// Supports nullable DateTime values.
/// </summary>
public class DateTimeToStringConverter : IValueConverter
{
    /// <summary>
    /// Default format string for date/time display.
    /// </summary>
    public string DefaultFormat { get; set; } = "g"; // Short date and time pattern

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Use parameter as format string if provided, otherwise use DefaultFormat
        var format = parameter as string ?? DefaultFormat;

        if (value is DateTime dateTime)
        {
            return dateTime.ToString(format, culture);
        }

        if (value is DateTime?)
        {
            var nullableDateTime = (DateTime?)value;
            if (nullableDateTime.HasValue)
            {
                return nullableDateTime.Value.ToString(format, culture);
            }
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string dateString && !string.IsNullOrWhiteSpace(dateString))
        {
            if (DateTime.TryParse(dateString, culture, DateTimeStyles.None, out var result))
            {
                return result;
            }
        }

        return targetType == typeof(DateTime?) ? (object?)null : DateTime.MinValue;
    }
}
