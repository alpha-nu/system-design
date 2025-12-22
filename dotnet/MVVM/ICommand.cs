namespace PasswordGenerator.MVVM;

/// <summary>
/// Represents a command that can be executed with optional parameter.
/// </summary>
public interface ICommand
{
    /// <summary>Event triggered when command's executable state changes.</summary>
    event EventHandler? CanExecuteChanged;

    /// <summary>Determines if the command can be executed.</summary>
    bool CanExecute(object? parameter = null);

    /// <summary>Executes the command.</summary>
    void Execute(object? parameter = null);
}

/// <summary>
/// Relay command implementation for MVVM.
/// </summary>
public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _executeAction;
    private readonly Func<object?, bool>? _canExecuteFunc;

    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Initializes a new instance of the RelayCommand class.
    /// </summary>
    /// <param name="executeAction">Action to execute.</param>
    /// <param name="canExecuteFunc">Optional function to determine if command can execute.</param>
    public RelayCommand(Action<object?> executeAction, Func<object?, bool>? canExecuteFunc = null)
    {
        _executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
        _canExecuteFunc = canExecuteFunc;
    }

    public bool CanExecute(object? parameter = null)
    {
        return _canExecuteFunc?.Invoke(parameter) ?? true;
    }

    public void Execute(object? parameter = null)
    {
        if (CanExecute(parameter))
        {
            _executeAction(parameter);
        }
    }

    /// <summary>Raises the CanExecuteChanged event.</summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
