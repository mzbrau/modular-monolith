using System.Globalization;
using System.Windows.Data;
using TicketSystem.Client.Wpf.Models;

namespace TicketSystem.Client.Wpf.Converters;

/// <summary>
/// Converts IssueStatus enum values to human-readable display strings.
/// </summary>
public class StatusToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IssueStatus status)
        {
            return string.Empty;
        }

        return status switch
        {
            IssueStatus.Open => "Open",
            IssueStatus.InProgress => "In Progress",
            IssueStatus.Blocked => "Blocked",
            IssueStatus.Resolved => "Resolved",
            IssueStatus.Closed => "Closed",
            _ => status.ToString()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string statusString)
        {
            return IssueStatus.Open;
        }

        return statusString switch
        {
            "Open" => IssueStatus.Open,
            "In Progress" => IssueStatus.InProgress,
            "Blocked" => IssueStatus.Blocked,
            "Resolved" => IssueStatus.Resolved,
            "Closed" => IssueStatus.Closed,
            _ => IssueStatus.Open
        };
    }
}
