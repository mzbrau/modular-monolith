using System.Globalization;
using System.Windows;
using TicketSystem.Client.Wpf.Converters;
using TicketSystem.Client.Wpf.Models;
using Xunit;

namespace TicketSystem.Client.Wpf.Tests.Converters;

public class StatusToStringConverterTests
{
    private readonly StatusToStringConverter _converter = new();

    [Theory]
    [InlineData(IssueStatus.Open, "Open")]
    [InlineData(IssueStatus.InProgress, "In Progress")]
    [InlineData(IssueStatus.Blocked, "Blocked")]
    [InlineData(IssueStatus.Resolved, "Resolved")]
    [InlineData(IssueStatus.Closed, "Closed")]
    public void Convert_ValidStatus_ReturnsCorrectString(IssueStatus status, string expected)
    {
        // Act
        var result = _converter.Convert(status, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_NullValue_ReturnsEmptyString()
    {
        // Act
        var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Convert_InvalidType_ReturnsEmptyString()
    {
        // Act
        var result = _converter.Convert("not a status", typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Theory]
    [InlineData("Open", IssueStatus.Open)]
    [InlineData("In Progress", IssueStatus.InProgress)]
    [InlineData("Blocked", IssueStatus.Blocked)]
    [InlineData("Resolved", IssueStatus.Resolved)]
    [InlineData("Closed", IssueStatus.Closed)]
    public void ConvertBack_ValidString_ReturnsCorrectStatus(string statusString, IssueStatus expected)
    {
        // Act
        var result = _converter.ConvertBack(statusString, typeof(IssueStatus), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertBack_InvalidString_ReturnsOpen()
    {
        // Act
        var result = _converter.ConvertBack("Invalid", typeof(IssueStatus), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(IssueStatus.Open, result);
    }
}

public class BoolToVisibilityConverterTests
{
    private readonly BoolToVisibilityConverter _converter = new();

    [Fact]
    public void Convert_True_ReturnsVisible()
    {
        // Act
        var result = _converter.Convert(true, typeof(Visibility), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_False_ReturnsCollapsed()
    {
        // Act
        var result = _converter.Convert(false, typeof(Visibility), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_NullValue_ReturnsCollapsed()
    {
        // Act
        var result = _converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_InvalidType_ReturnsCollapsed()
    {
        // Act
        var result = _converter.Convert("not a bool", typeof(Visibility), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void ConvertBack_Visible_ReturnsTrue()
    {
        // Act
        var result = _converter.ConvertBack(Visibility.Visible, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(true, result);
    }

    [Fact]
    public void ConvertBack_Collapsed_ReturnsFalse()
    {
        // Act
        var result = _converter.ConvertBack(Visibility.Collapsed, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(false, result);
    }

    [Fact]
    public void ConvertBack_Hidden_ReturnsFalse()
    {
        // Act
        var result = _converter.ConvertBack(Visibility.Hidden, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(false, result);
    }
}

public class DateTimeToStringConverterTests
{
    private readonly DateTimeToStringConverter _converter = new();

    [Fact]
    public void Convert_DateTime_ReturnsFormattedString()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 15, 14, 30, 0);

        // Act
        var result = _converter.Convert(dateTime, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<string>(result);
        Assert.NotEmpty((string)result);
    }

    [Fact]
    public void Convert_DateTimeWithCustomFormat_ReturnsFormattedString()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 15, 14, 30, 0);
        var format = "yyyy-MM-dd";

        // Act
        var result = _converter.Convert(dateTime, typeof(string), format, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("2024-01-15", result);
    }

    [Fact]
    public void Convert_NullableDateTime_ReturnsFormattedString()
    {
        // Arrange
        DateTime? dateTime = new DateTime(2024, 1, 15, 14, 30, 0);

        // Act
        var result = _converter.Convert(dateTime, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<string>(result);
        Assert.NotEmpty((string)result);
    }

    [Fact]
    public void Convert_NullDateTime_ReturnsEmptyString()
    {
        // Arrange
        DateTime? dateTime = null;

        // Act
        var result = _converter.Convert(dateTime, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Convert_InvalidType_ReturnsEmptyString()
    {
        // Act
        var result = _converter.Convert("not a date", typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ConvertBack_ValidDateString_ReturnsDateTime()
    {
        // Arrange
        var dateString = "2024-01-15";

        // Act
        var result = _converter.ConvertBack(dateString, typeof(DateTime), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<DateTime>(result);
        var dateTime = (DateTime)result;
        Assert.Equal(2024, dateTime.Year);
        Assert.Equal(1, dateTime.Month);
        Assert.Equal(15, dateTime.Day);
    }

    [Fact]
    public void ConvertBack_InvalidDateString_ReturnsMinValue()
    {
        // Act
        var result = _converter.ConvertBack("invalid date", typeof(DateTime), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(DateTime.MinValue, result);
    }

    [Fact]
    public void ConvertBack_EmptyString_ReturnsMinValue()
    {
        // Act
        var result = _converter.ConvertBack(string.Empty, typeof(DateTime), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(DateTime.MinValue, result);
    }

    [Fact]
    public void ConvertBack_NullableTargetType_ReturnsNull()
    {
        // Act
        var result = _converter.ConvertBack("invalid date", typeof(DateTime?), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DefaultFormat_CanBeChanged()
    {
        // Arrange
        var converter = new DateTimeToStringConverter { DefaultFormat = "yyyy-MM-dd" };
        var dateTime = new DateTime(2024, 1, 15);

        // Act
        var result = converter.Convert(dateTime, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("2024-01-15", result);
    }
}
