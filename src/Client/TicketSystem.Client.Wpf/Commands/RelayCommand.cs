using System.Windows.Input;

namespace TicketSystem.Client.Wpf.Commands;

/// <summary>
/// A command implementation that relays its functionality to delegates.
/// Encapsulates synchronous command logic with optional can-execute validation.
/// </summary>
/// <remarks>
/// Requirements: 14.5 - THE WPF_Application SHALL bind commands to UI buttons and controls using ICommand implementations
/// </remarks>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    /// <summary>
    /// Initializes a new instance of the RelayCommand class.
    /// </summary>
    /// <param name="execute">The execution logic. Cannot be null.</param>
    /// <param name="canExecute">The execution status logic. Can be null if always executable.</param>
    /// <exception cref="ArgumentNullException">Thrown when execute is null.</exception>
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
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
    /// </summary>
    /// <param name="parameter">Data used by the command. Can be null.</param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>
    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    /// <summary>
    /// Executes the command logic.
    /// </summary>
    /// <param name="parameter">Data used by the command. Can be null.</param>
    public void Execute(object? parameter)
    {
        _execute(parameter);
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
