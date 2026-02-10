using TicketSystem.Client.Wpf.ViewModels;

namespace TicketSystem.Client.Wpf.Services;

/// <summary>
/// Service for managing navigation between different views in the application.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates to the view associated with the specified ViewModel type.
    /// </summary>
    /// <typeparam name="TViewModel">The type of ViewModel to navigate to.</typeparam>
    void NavigateTo<TViewModel>() where TViewModel : BaseViewModel;

    /// <summary>
    /// Navigates to the view associated with the specified ViewModel type.
    /// </summary>
    /// <param name="viewModelType">The type of ViewModel to navigate to.</param>
    void NavigateTo(Type viewModelType);

    /// <summary>
    /// Gets the currently active ViewModel.
    /// </summary>
    BaseViewModel? CurrentViewModel { get; }

    /// <summary>
    /// Event raised when the current ViewModel changes.
    /// </summary>
    event EventHandler<BaseViewModel>? CurrentViewModelChanged;
}
