using Microsoft.Extensions.DependencyInjection;
using TicketSystem.Client.Wpf.ViewModels;

namespace TicketSystem.Client.Wpf.Services;

/// <summary>
/// Implementation of navigation service that manages view navigation using dependency injection.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private BaseViewModel? _currentViewModel;

    /// <summary>
    /// Initializes a new instance of the NavigationService class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving ViewModels.</param>
    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc/>
    public BaseViewModel? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            if (_currentViewModel != value)
            {
                _currentViewModel = value;
                if (value != null)
                {
                    CurrentViewModelChanged?.Invoke(this, value);
                }
            }
        }
    }

    /// <inheritdoc/>
    public event EventHandler<BaseViewModel>? CurrentViewModelChanged;

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        CurrentViewModel = viewModel;
    }

    /// <inheritdoc/>
    public void NavigateTo(Type viewModelType)
    {
        if (viewModelType == null)
        {
            throw new ArgumentNullException(nameof(viewModelType));
        }

        if (!typeof(BaseViewModel).IsAssignableFrom(viewModelType))
        {
            throw new ArgumentException(
                $"Type {viewModelType.Name} must inherit from BaseViewModel.",
                nameof(viewModelType));
        }

        var viewModel = _serviceProvider.GetRequiredService(viewModelType) as BaseViewModel;
        CurrentViewModel = viewModel;
    }
}
