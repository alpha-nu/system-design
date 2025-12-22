namespace PasswordGenerator.MVP;

/// <summary>
/// Interface defining the Passive View contract.
/// The View exposes events for user actions and methods for updates, but has NO knowledge of the Model.
/// </summary>
public interface IPasswordView
{
    /// <summary>Event triggered when user requests password generation.</summary>
    event EventHandler? OnGenerateRequested;

    /// <summary>Event triggered when user requests password save.</summary>
    event EventHandler? OnSaveRequested;

    /// <summary>Event triggered when user requests to view all passwords.</summary>
    event EventHandler? OnViewAllRequested;

    /// <summary>Event triggered when user requests password deletion.</summary>
    event EventHandler? OnDeleteRequested;

    /// <summary>Event triggered when user requests to exit.</summary>
    event EventHandler? OnExitRequested;

    /// <summary>Displays the main menu.</summary>
    void ShowMainMenu();

    /// <summary>Displays an error message.</summary>
    void ShowError(string message);

    /// <summary>Displays a success message.</summary>
    void ShowSuccess(string message);

    /// <summary>Displays an info message.</summary>
    void ShowInfo(string message);

    /// <summary>Sets available password length input.</summary>
    int PasswordLength { get; set; }

    /// <summary>Sets whether to use special characters.</summary>
    bool UseSpecialChars { get; set; }

    /// <summary>Sets available random sources for selection.</summary>
    void SetAvailableRandomSources(IEnumerable<(RandomSourceType Type, string Name)> sources);

    /// <summary>Gets the selected random source index.</summary>
    int SelectedRandomSourceIndex { get; set; }

    /// <summary>Displays the generated password.</summary>
    void DisplayGeneratedPassword(string password, PasswordStrength strength);

    /// <summary>Gets the password key for saving.</summary>
    string PasswordKey { get; set; }

    /// <summary>Gets the password description.</summary>
    string PasswordDescription { get; set; }

    /// <summary>Displays all saved passwords.</summary>
    void DisplayPasswordList(IEnumerable<PasswordEntry> entries);

    /// <summary>Gets the password key to delete.</summary>
    string DeletePasswordKey { get; set; }
}
