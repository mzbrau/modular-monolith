using System.ComponentModel;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using TicketSystem.Client.Wpf.ViewModels;

namespace TicketSystem.Client.Wpf.Tests.Properties;

/// <summary>
/// Property-based tests for BaseViewModel class.
/// Feature: wpf-ticket-client
/// </summary>
public class BaseViewModelPropertyTests
{
    /// <summary>
    /// Test ViewModel that exposes multiple properties for property-based testing.
    /// </summary>
    private class TestViewModel : BaseViewModel
    {
        private string? _stringProperty;
        private int _intProperty;
        private bool _boolProperty;
        private DateTime _dateTimeProperty;

        public string? StringProperty
        {
            get => _stringProperty;
            set => SetProperty(ref _stringProperty, value);
        }

        public int IntProperty
        {
            get => _intProperty;
            set => SetProperty(ref _intProperty, value);
        }

        public bool BoolProperty
        {
            get => _boolProperty;
            set => SetProperty(ref _boolProperty, value);
        }

        public DateTime DateTimeProperty
        {
            get => _dateTimeProperty;
            set => SetProperty(ref _dateTimeProperty, value);
        }
    }

    /// <summary>
    /// Property 11: ViewModel Property Change Notification
    /// 
    /// **Validates: Requirements 14.1, 14.2**
    /// 
    /// For all ViewModels, they SHALL implement INotifyPropertyChanged, and when any property changes,
    /// the PropertyChanged event SHALL be raised with the correct property name.
    /// 
    /// This property test verifies that:
    /// 1. PropertyChanged event is raised when a property value changes
    /// 2. The event contains the correct property name
    /// 3. The event is NOT raised when the value doesn't change
    /// 4. This holds for ANY property type (string, int, bool, DateTime, etc.)
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Property11_ViewModelPropertyChangeNotification_StringProperty(string? newValue)
    {
        return Prop.ForAll(
            Arb.Default.String().Generator.ToArbitrary(),
            initialValue =>
            {
                // Arrange
                var viewModel = new TestViewModel { StringProperty = initialValue };
                var eventRaised = false;
                string? raisedPropertyName = null;

                viewModel.PropertyChanged += (sender, args) =>
                {
                    eventRaised = true;
                    raisedPropertyName = args.PropertyName;
                };

                // Act
                viewModel.StringProperty = newValue;

                // Assert
                if (EqualityComparer<string?>.Default.Equals(initialValue, newValue))
                {
                    // When value doesn't change, event should NOT be raised
                    return (!eventRaised).Label($"Event should not be raised when value doesn't change (initial: {initialValue}, new: {newValue})");
                }
                else
                {
                    // When value changes, event SHOULD be raised with correct property name
                    return (eventRaised && raisedPropertyName == nameof(TestViewModel.StringProperty))
                        .Label($"Event should be raised with correct property name when value changes (initial: {initialValue}, new: {newValue}, eventRaised: {eventRaised}, propertyName: {raisedPropertyName})");
                }
            });
    }

    /// <summary>
    /// Property 11: ViewModel Property Change Notification (Int Property)
    /// 
    /// **Validates: Requirements 14.1, 14.2**
    /// 
    /// Tests property change notification for integer properties.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Property11_ViewModelPropertyChangeNotification_IntProperty(int newValue)
    {
        return Prop.ForAll(
            Arb.Default.Int32().Generator.ToArbitrary(),
            initialValue =>
            {
                // Arrange
                var viewModel = new TestViewModel { IntProperty = initialValue };
                var eventRaised = false;
                string? raisedPropertyName = null;

                viewModel.PropertyChanged += (sender, args) =>
                {
                    eventRaised = true;
                    raisedPropertyName = args.PropertyName;
                };

                // Act
                viewModel.IntProperty = newValue;

                // Assert
                if (initialValue == newValue)
                {
                    return (!eventRaised).Label($"Event should not be raised when value doesn't change (initial: {initialValue}, new: {newValue})");
                }
                else
                {
                    return (eventRaised && raisedPropertyName == nameof(TestViewModel.IntProperty))
                        .Label($"Event should be raised with correct property name when value changes (initial: {initialValue}, new: {newValue}, eventRaised: {eventRaised}, propertyName: {raisedPropertyName})");
                }
            });
    }

    /// <summary>
    /// Property 11: ViewModel Property Change Notification (Bool Property)
    /// 
    /// **Validates: Requirements 14.1, 14.2**
    /// 
    /// Tests property change notification for boolean properties.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Property11_ViewModelPropertyChangeNotification_BoolProperty(bool newValue)
    {
        return Prop.ForAll(
            Arb.Default.Bool().Generator.ToArbitrary(),
            initialValue =>
            {
                // Arrange
                var viewModel = new TestViewModel { BoolProperty = initialValue };
                var eventRaised = false;
                string? raisedPropertyName = null;

                viewModel.PropertyChanged += (sender, args) =>
                {
                    eventRaised = true;
                    raisedPropertyName = args.PropertyName;
                };

                // Act
                viewModel.BoolProperty = newValue;

                // Assert
                if (initialValue == newValue)
                {
                    return (!eventRaised).Label($"Event should not be raised when value doesn't change (initial: {initialValue}, new: {newValue})");
                }
                else
                {
                    return (eventRaised && raisedPropertyName == nameof(TestViewModel.BoolProperty))
                        .Label($"Event should be raised with correct property name when value changes (initial: {initialValue}, new: {newValue}, eventRaised: {eventRaised}, propertyName: {raisedPropertyName})");
                }
            });
    }

    /// <summary>
    /// Property 11: ViewModel Property Change Notification (DateTime Property)
    /// 
    /// **Validates: Requirements 14.1, 14.2**
    /// 
    /// Tests property change notification for DateTime properties.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Property11_ViewModelPropertyChangeNotification_DateTimeProperty(DateTime newValue)
    {
        return Prop.ForAll(
            Arb.Default.DateTime().Generator.ToArbitrary(),
            initialValue =>
            {
                // Arrange
                var viewModel = new TestViewModel { DateTimeProperty = initialValue };
                var eventRaised = false;
                string? raisedPropertyName = null;

                viewModel.PropertyChanged += (sender, args) =>
                {
                    eventRaised = true;
                    raisedPropertyName = args.PropertyName;
                };

                // Act
                viewModel.DateTimeProperty = newValue;

                // Assert
                if (initialValue == newValue)
                {
                    return (!eventRaised).Label($"Event should not be raised when value doesn't change (initial: {initialValue}, new: {newValue})");
                }
                else
                {
                    return (eventRaised && raisedPropertyName == nameof(TestViewModel.DateTimeProperty))
                        .Label($"Event should be raised with correct property name when value changes (initial: {initialValue}, new: {newValue}, eventRaised: {eventRaised}, propertyName: {raisedPropertyName})");
                }
            });
    }

    /// <summary>
    /// Property 11: ViewModel Property Change Notification (IsLoading Property)
    /// 
    /// **Validates: Requirements 14.1, 14.2**
    /// 
    /// Tests property change notification for the built-in IsLoading property.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Property11_ViewModelPropertyChangeNotification_IsLoadingProperty(bool newValue)
    {
        return Prop.ForAll(
            Arb.Default.Bool().Generator.ToArbitrary(),
            initialValue =>
            {
                // Arrange
                var viewModel = new TestViewModel { IsLoading = initialValue };
                var eventRaised = false;
                string? raisedPropertyName = null;

                viewModel.PropertyChanged += (sender, args) =>
                {
                    eventRaised = true;
                    raisedPropertyName = args.PropertyName;
                };

                // Act
                viewModel.IsLoading = newValue;

                // Assert
                if (initialValue == newValue)
                {
                    return (!eventRaised).Label($"Event should not be raised when value doesn't change (initial: {initialValue}, new: {newValue})");
                }
                else
                {
                    return (eventRaised && raisedPropertyName == nameof(BaseViewModel.IsLoading))
                        .Label($"Event should be raised with correct property name when value changes (initial: {initialValue}, new: {newValue}, eventRaised: {eventRaised}, propertyName: {raisedPropertyName})");
                }
            });
    }

    /// <summary>
    /// Property 11: ViewModel Property Change Notification (ErrorMessage Property)
    /// 
    /// **Validates: Requirements 14.1, 14.2**
    /// 
    /// Tests property change notification for the built-in ErrorMessage property.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property Property11_ViewModelPropertyChangeNotification_ErrorMessageProperty(string? newValue)
    {
        return Prop.ForAll(
            Arb.Default.String().Generator.ToArbitrary(),
            initialValue =>
            {
                // Arrange
                var viewModel = new TestViewModel { ErrorMessage = initialValue };
                var eventRaised = false;
                string? raisedPropertyName = null;

                viewModel.PropertyChanged += (sender, args) =>
                {
                    eventRaised = true;
                    raisedPropertyName = args.PropertyName;
                };

                // Act
                viewModel.ErrorMessage = newValue;

                // Assert
                if (EqualityComparer<string?>.Default.Equals(initialValue, newValue))
                {
                    return (!eventRaised).Label($"Event should not be raised when value doesn't change (initial: {initialValue}, new: {newValue})");
                }
                else
                {
                    return (eventRaised && raisedPropertyName == nameof(BaseViewModel.ErrorMessage))
                        .Label($"Event should be raised with correct property name when value changes (initial: {initialValue}, new: {newValue}, eventRaised: {eventRaised}, propertyName: {raisedPropertyName})");
                }
            });
    }
}
