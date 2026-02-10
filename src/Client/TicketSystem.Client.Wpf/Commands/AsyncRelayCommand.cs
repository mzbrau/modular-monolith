using System.Windows.Input;

namespace TicketSystem.Client.Wpf.Commands;

/// <summary>
/// A command implementation that relays its functionality to async delegates.
/// Encapsulates asynchronous command logic with optional can-execute validation.
/// Prevents concurrent execution and handles async exceptions.
/// </summary>
/// <remarks>
/// Requirements:
/// - 13.1: WHEN the WPF_Application performs a gRPC call, THE WPF_Application SHALL execute it asynchronously
/// - 13.5: THE WPF_Application SHALL use async/await patterns for all I/O operations
/// - 14.5: THE WPF_Application SHALL bind commands to UI buttons and controls using ICommand implementations
/// </remarks>
public class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _execute;
    private readonly Func<object?, bool>? _canExecute;
    private bool _isExecuting;

    /// <summary>
    /// Initializes a new instance of the AsyncRelayCommand class.
    /// </summary>
    /// <param name="execute">The async execution logic. Cannot be null.</param>
    /// <param name="canExecute">The execution status logic. Can be null if always executable.</param>
    /// <exception cref="ArgumentNullException">Thrown when execute is null.</exception>
    public AsyncRelayCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Occurs when changes occur that affect whether or not the command should execute.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Determines whether the command can execute in its current state.
    /// Prevents concurrent execution by returning false when already executing.
    /// </summary>
    /// <param name="parameter">Data used by the command. Can be null.</param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>
    public bool CanExecute(object? parameter)
    {
        // Prevent concurrent execution
        if (_isExecuting)
        {
            return false;
        }

        return _canExecute == null || _canExecute(parameter);
    }

    /// <summary>
    /// Executes the async command logic.
    /// Prevents concurrent execution and handles async exceptions.
    /// </summary>
    /// <param name="parameter">Data used by the command. Can be null.</param>
    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        _isExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            await _execute(parameter);
        }
        catch (Exception ex)
        {
            // Handle async exceptions - in a real application, this might log the error
            // or notify the user through a centralized error handling mechanism
            // For now, we'll let the exception propagate to the synchronization context
            // which will typically show it in the debugger or crash the app
            System.Diagnostics.Debug.WriteLine($"AsyncRelayCommand exception: {ex}");
            throw;
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Raises the CanExecuteChanged event to notify the UI that the command's execution status has changed.
    /// Call this method when conditions that affect CanExecute have changed.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
