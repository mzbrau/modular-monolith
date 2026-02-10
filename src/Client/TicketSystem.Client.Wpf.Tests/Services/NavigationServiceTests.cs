using Microsoft.Extensions.DependencyInjection;
using Moq;
using TicketSystem.Client.Wpf.Services;
using TicketSystem.Client.Wpf.ViewModels;

namespace TicketSystem.Client.Wpf.Tests.Services;

/// <summary>
/// Unit tests for NavigationService.
/// Tests ViewModel resolution from DI container, CurrentViewModel updates, and event raising.
/// </summary>
public class NavigationServiceTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly NavigationService _navigationService;

    public NavigationServiceTests()
    {
        // Set up a real service provider with test ViewModels
        var services = new ServiceCollection();
        services.AddTransient<TestViewModel1>();
        services.AddTransient<TestViewModel2>();
        _serviceProvider = services.BuildServiceProvider();

        _navigationService = new NavigationService(_serviceProvider);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new NavigationService(null!));
    }

    [Fact]
    public void Constructor_WithValidServiceProvider_InitializesSuccessfully()
    {
        // Act
        var service = new NavigationService(_serviceProvider);

        // Assert
        Assert.NotNull(service);
        Assert.Null(service.CurrentViewModel);
    }

    #endregion

    #region NavigateTo<T> Tests

    [Fact]
    public void NavigateTo_Generic_ResolvesViewModelFromDIContainer()
    {
        // Act
        _navigationService.NavigateTo<TestViewModel1>();

        // Assert
        Assert.NotNull(_navigationService.CurrentViewModel);
        Assert.IsType<TestViewModel1>(_navigationService.CurrentViewModel);
    }

    [Fact]
    public void NavigateTo_Generic_UpdatesCurrentViewModel()
    {
        // Arrange
        _navigationService.NavigateTo<TestViewModel1>();
        var firstViewModel = _navigationService.CurrentViewModel;

        // Act
        _navigationService.NavigateTo<TestViewModel2>();

        // Assert
        Assert.NotNull(_navigationService.CurrentViewModel);
        Assert.IsType<TestViewModel2>(_navigationService.CurrentViewModel);
        Assert.NotSame(firstViewModel, _navigationService.CurrentViewModel);
    }

    [Fact]
    public void NavigateTo_Generic_RaisesCurrentViewModelChangedEvent()
    {
        // Arrange
        BaseViewModel? eventViewModel = null;
        _navigationService.CurrentViewModelChanged += (sender, vm) => eventViewModel = vm;

        // Act
        _navigationService.NavigateTo<TestViewModel1>();

        // Assert
        Assert.NotNull(eventViewModel);
        Assert.IsType<TestViewModel1>(eventViewModel);
        Assert.Same(_navigationService.CurrentViewModel, eventViewModel);
    }

    [Fact]
    public void NavigateTo_Generic_WhenCalledMultipleTimes_RaisesEventEachTime()
    {
        // Arrange
        int eventCount = 0;
        _navigationService.CurrentViewModelChanged += (sender, vm) => eventCount++;

        // Act
        _navigationService.NavigateTo<TestViewModel1>();
        _navigationService.NavigateTo<TestViewModel2>();
        _navigationService.NavigateTo<TestViewModel1>();

        // Assert
        Assert.Equal(3, eventCount);
    }

    [Fact]
    public void NavigateTo_Generic_WithUnregisteredViewModel_ThrowsInvalidOperationException()
    {
        // Arrange
        var emptyServiceProvider = new ServiceCollection().BuildServiceProvider();
        var service = new NavigationService(emptyServiceProvider);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => service.NavigateTo<TestViewModel1>());
    }

    #endregion

    #region NavigateTo(Type) Tests

    [Fact]
    public void NavigateTo_Type_ResolvesViewModelFromDIContainer()
    {
        // Act
        _navigationService.NavigateTo(typeof(TestViewModel1));

        // Assert
        Assert.NotNull(_navigationService.CurrentViewModel);
        Assert.IsType<TestViewModel1>(_navigationService.CurrentViewModel);
    }

    [Fact]
    public void NavigateTo_Type_UpdatesCurrentViewModel()
    {
        // Arrange
        _navigationService.NavigateTo(typeof(TestViewModel1));
        var firstViewModel = _navigationService.CurrentViewModel;

        // Act
        _navigationService.NavigateTo(typeof(TestViewModel2));

        // Assert
        Assert.NotNull(_navigationService.CurrentViewModel);
        Assert.IsType<TestViewModel2>(_navigationService.CurrentViewModel);
        Assert.NotSame(firstViewModel, _navigationService.CurrentViewModel);
    }

    [Fact]
    public void NavigateTo_Type_RaisesCurrentViewModelChangedEvent()
    {
        // Arrange
        BaseViewModel? eventViewModel = null;
        _navigationService.CurrentViewModelChanged += (sender, vm) => eventViewModel = vm;

        // Act
        _navigationService.NavigateTo(typeof(TestViewModel1));

        // Assert
        Assert.NotNull(eventViewModel);
        Assert.IsType<TestViewModel1>(eventViewModel);
        Assert.Same(_navigationService.CurrentViewModel, eventViewModel);
    }

    [Fact]
    public void NavigateTo_Type_WithNullType_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _navigationService.NavigateTo(null!));
    }

    [Fact]
    public void NavigateTo_Type_WithNonViewModelType_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _navigationService.NavigateTo(typeof(string)));
        Assert.Contains("must inherit from BaseViewModel", exception.Message);
    }

    [Fact]
    public void NavigateTo_Type_WithUnregisteredViewModel_ThrowsInvalidOperationException()
    {
        // Arrange
        var emptyServiceProvider = new ServiceCollection().BuildServiceProvider();
        var service = new NavigationService(emptyServiceProvider);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => service.NavigateTo(typeof(TestViewModel1)));
    }

    #endregion

    #region CurrentViewModel Property Tests

    [Fact]
    public void CurrentViewModel_InitiallyNull()
    {
        // Arrange
        var service = new NavigationService(_serviceProvider);

        // Assert
        Assert.Null(service.CurrentViewModel);
    }

    [Fact]
    public void CurrentViewModel_AfterNavigation_ReturnsCorrectViewModel()
    {
        // Act
        _navigationService.NavigateTo<TestViewModel1>();

        // Assert
        Assert.NotNull(_navigationService.CurrentViewModel);
        Assert.IsType<TestViewModel1>(_navigationService.CurrentViewModel);
    }

    [Fact]
    public void CurrentViewModel_AfterMultipleNavigations_ReturnsLatestViewModel()
    {
        // Act
        _navigationService.NavigateTo<TestViewModel1>();
        _navigationService.NavigateTo<TestViewModel2>();

        // Assert
        Assert.IsType<TestViewModel2>(_navigationService.CurrentViewModel);
    }

    #endregion

    #region Event Tests

    [Fact]
    public void CurrentViewModelChanged_NotRaisedWhenNoNavigation()
    {
        // Arrange
        int eventCount = 0;
        _navigationService.CurrentViewModelChanged += (sender, vm) => eventCount++;

        // Act
        // No navigation

        // Assert
        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void CurrentViewModelChanged_PassesCorrectSenderAndViewModel()
    {
        // Arrange
        object? eventSender = null;
        BaseViewModel? eventViewModel = null;
        _navigationService.CurrentViewModelChanged += (sender, vm) =>
        {
            eventSender = sender;
            eventViewModel = vm;
        };

        // Act
        _navigationService.NavigateTo<TestViewModel1>();

        // Assert
        Assert.Same(_navigationService, eventSender);
        Assert.Same(_navigationService.CurrentViewModel, eventViewModel);
    }

    [Fact]
    public void CurrentViewModelChanged_SupportsMultipleSubscribers()
    {
        // Arrange
        int subscriber1Count = 0;
        int subscriber2Count = 0;
        _navigationService.CurrentViewModelChanged += (sender, vm) => subscriber1Count++;
        _navigationService.CurrentViewModelChanged += (sender, vm) => subscriber2Count++;

        // Act
        _navigationService.NavigateTo<TestViewModel1>();

        // Assert
        Assert.Equal(1, subscriber1Count);
        Assert.Equal(1, subscriber2Count);
    }

    [Fact]
    public void CurrentViewModelChanged_CanUnsubscribe()
    {
        // Arrange
        int eventCount = 0;
        EventHandler<BaseViewModel> handler = (sender, vm) => eventCount++;
        _navigationService.CurrentViewModelChanged += handler;
        _navigationService.NavigateTo<TestViewModel1>();

        // Act
        _navigationService.CurrentViewModelChanged -= handler;
        _navigationService.NavigateTo<TestViewModel2>();

        // Assert
        Assert.Equal(1, eventCount); // Only the first navigation should have triggered the event
    }

    #endregion

    #region Test ViewModels

    /// <summary>
    /// Test ViewModel for testing navigation.
    /// </summary>
    private class TestViewModel1 : BaseViewModel
    {
    }

    /// <summary>
    /// Test ViewModel for testing navigation.
    /// </summary>
    private class TestViewModel2 : BaseViewModel
    {
    }

    #endregion
}
