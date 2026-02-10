using System.ComponentModel;
using FluentAssertions;
using TicketSystem.Client.Wpf.ViewModels;

namespace TicketSystem.Client.Wpf.Tests.ViewModels;

/// <summary>
/// Unit tests for BaseViewModel class.
/// Tests Requirements 14.1, 14.2 - INotifyPropertyChanged implementation.
/// </summary>
public class BaseViewModelTests
{
    /// <summary>
    /// Test ViewModel that exposes protected methods for testing.
    /// </summary>
    private class TestViewModel : BaseViewModel
    {
        private string? _testProperty;

        public string? TestProperty
        {
            get => _testProperty;
            set => SetProperty(ref _testProperty, value);
        }

        public void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
    }

    [Fact]
    public void BaseViewModel_ShouldImplementINotifyPropertyChanged()
    {
        // Arrange & Act
        var viewModel = new TestViewModel();

        // Assert
        viewModel.Should().BeAssignableTo<INotifyPropertyChanged>();
    }

    [Fact]
    public void OnPropertyChanged_ShouldRaisePropertyChangedEvent()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var eventRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.RaisePropertyChanged("TestProperty");

        // Assert
        eventRaised.Should().BeTrue();
        raisedPropertyName.Should().Be("TestProperty");
    }

    [Fact]
    public void OnPropertyChanged_WithNullPropertyName_ShouldRaiseEventWithNull()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var eventRaised = false;
        string? raisedPropertyName = "NotNull";

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.RaisePropertyChanged(null!);

        // Assert
        eventRaised.Should().BeTrue();
        raisedPropertyName.Should().BeNull();
    }

    [Fact]
    public void SetProperty_WhenValueChanges_ShouldUpdateFieldAndRaiseEvent()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var eventRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.TestProperty = "NewValue";

        // Assert
        viewModel.TestProperty.Should().Be("NewValue");
        eventRaised.Should().BeTrue();
        raisedPropertyName.Should().Be(nameof(TestViewModel.TestProperty));
    }

    [Fact]
    public void SetProperty_WhenValueDoesNotChange_ShouldNotRaiseEvent()
    {
        // Arrange
        var viewModel = new TestViewModel { TestProperty = "InitialValue" };
        var eventRaisedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaisedCount++;
        };

        // Act
        viewModel.TestProperty = "InitialValue";

        // Assert
        viewModel.TestProperty.Should().Be("InitialValue");
        eventRaisedCount.Should().Be(0);
    }

    [Fact]
    public void SetProperty_WhenValueChanges_ShouldReturnTrue()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var field = "OldValue";

        // Act
        var result = viewModel.GetType()
            .GetMethod("SetProperty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .MakeGenericMethod(typeof(string))
            .Invoke(viewModel, new object?[] { field, "NewValue", "TestProperty" });

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void IsLoading_WhenSet_ShouldRaisePropertyChangedEvent()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var eventRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.IsLoading = true;

        // Assert
        viewModel.IsLoading.Should().BeTrue();
        eventRaised.Should().BeTrue();
        raisedPropertyName.Should().Be(nameof(BaseViewModel.IsLoading));
    }

    [Fact]
    public void IsLoading_DefaultValue_ShouldBeFalse()
    {
        // Arrange & Act
        var viewModel = new TestViewModel();

        // Assert
        viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void IsLoading_WhenSetToSameValue_ShouldNotRaiseEvent()
    {
        // Arrange
        var viewModel = new TestViewModel { IsLoading = false };
        var eventRaisedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaisedCount++;
        };

        // Act
        viewModel.IsLoading = false;

        // Assert
        eventRaisedCount.Should().Be(0);
    }

    [Fact]
    public void ErrorMessage_WhenSet_ShouldRaisePropertyChangedEvent()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var eventRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaised = true;
            raisedPropertyName = args.PropertyName;
        };

        // Act
        viewModel.ErrorMessage = "An error occurred";

        // Assert
        viewModel.ErrorMessage.Should().Be("An error occurred");
        eventRaised.Should().BeTrue();
        raisedPropertyName.Should().Be(nameof(BaseViewModel.ErrorMessage));
    }

    [Fact]
    public void ErrorMessage_DefaultValue_ShouldBeNull()
    {
        // Arrange & Act
        var viewModel = new TestViewModel();

        // Assert
        viewModel.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ErrorMessage_WhenSetToNull_ShouldRaisePropertyChangedEvent()
    {
        // Arrange
        var viewModel = new TestViewModel { ErrorMessage = "Error" };
        var eventRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaised = true;
        };

        // Act
        viewModel.ErrorMessage = null;

        // Assert
        viewModel.ErrorMessage.Should().BeNull();
        eventRaised.Should().BeTrue();
    }

    [Fact]
    public void ErrorMessage_WhenSetToSameValue_ShouldNotRaiseEvent()
    {
        // Arrange
        var viewModel = new TestViewModel { ErrorMessage = "Error" };
        var eventRaisedCount = 0;

        viewModel.PropertyChanged += (sender, args) =>
        {
            eventRaisedCount++;
        };

        // Act
        viewModel.ErrorMessage = "Error";

        // Assert
        eventRaisedCount.Should().Be(0);
    }

    [Fact]
    public void SetProperty_WithMultipleProperties_ShouldRaiseCorrectEvents()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var propertyNames = new List<string>();

        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                propertyNames.Add(args.PropertyName);
            }
        };

        // Act
        viewModel.IsLoading = true;
        viewModel.ErrorMessage = "Error";
        viewModel.TestProperty = "Value";

        // Assert
        propertyNames.Should().HaveCount(3);
        propertyNames.Should().Contain(nameof(BaseViewModel.IsLoading));
        propertyNames.Should().Contain(nameof(BaseViewModel.ErrorMessage));
        propertyNames.Should().Contain(nameof(TestViewModel.TestProperty));
    }
}
