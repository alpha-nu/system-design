namespace PasswordGenerator.MVVM;

/// <summary>
/// Interface for property change notifications in MVVM.
/// </summary>
public interface INotifyPropertyChanged
{
    /// <summary>Event triggered when a property changes.</summary>
    event EventHandler<PropertyChangedEventArgs>? PropertyChanged;
}

/// <summary>
/// Event arguments for property changes.
/// </summary>
public sealed class PropertyChangedEventArgs : EventArgs
{
    /// <summary>Gets the name of the changed property.</summary>
    public string PropertyName { get; }

    /// <summary>Initializes a new instance of PropertyChangedEventArgs.</summary>
    public PropertyChangedEventArgs(string propertyName)
    {
        PropertyName = propertyName;
    }
}
