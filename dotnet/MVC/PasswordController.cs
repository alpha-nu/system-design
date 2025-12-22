using Generator = PasswordGenerator.Shared.PasswordGenerator;

namespace PasswordGenerator.MVC;

/// <summary>
/// Supervising MVC Controller.
/// Orchestrates interaction between Model and View, handles all application logic flow.
/// </summary>
public sealed class PasswordController
{
    private readonly PasswordModel _model;
    private readonly ConsoleView _view;

    // Current state
    private string? _currentPassword;
    private RandomSourceType _currentRandomSource = RandomSourceType.CryptoRandom;

    /// <summary>
    /// Initializes a new instance of the PasswordController class.
    /// </summary>
    /// <param name="model">The password model.</param>
    /// <param name="view">The console view.</param>
    public PasswordController(PasswordModel model, ConsoleView view)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _view = view ?? throw new ArgumentNullException(nameof(view));
    }

    /// <summary>
    /// Runs the main application loop.
    /// </summary>
    public void Run()
    {
        while (true)
        {
            try
            {
                var choice = _view.ShowMainMenu();

                switch (choice)
                {
                    case '1':
                        HandleGeneratePassword();
                        break;
                    case '2':
                        HandleSavePassword();
                        break;
                    case '3':
                        HandleViewPasswords();
                        break;
                    case '4':
                        HandleDeletePassword();
                        break;
                    case '5':
                        _view.ShowGoodbye();
                        return;
                    default:
                        _view.ShowError("Invalid option. Please select 1-5.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _view.ShowError($"An unexpected error occurred: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Handles password generation flow.
    /// </summary>
    private void HandleGeneratePassword()
    {
        try
        {
            // Get user inputs
            var length = _view.GetPasswordLength();
            var useSpecialChars = _view.GetUseSpecialChars();
            var sources = _model.GetAvailableRandomSources();
            var sourceIndex = _view.GetRandomSourceChoice(sources);

            if (sourceIndex < 0)
                return;

            _currentRandomSource = sources.ElementAt(sourceIndex).Type;

            // Generate password via model
            var config = new PasswordConfig(length, useSpecialChars, _currentRandomSource);
            var randomSource = RandomSourceFactory.Create(_currentRandomSource);
            var generator = new Generator(randomSource);

            _currentPassword = generator.Generate(config);
            var strength = _model.GetPasswordStrength(_currentPassword);

            _view.ShowGeneratedPassword(_currentPassword, strength);
        }
        catch (Exception ex)
        {
            _view.ShowError(ex.Message);
        }
    }

    /// <summary>
    /// Handles password save flow.
    /// </summary>
    private void HandleSavePassword()
    {
        try
        {
            if (string.IsNullOrEmpty(_currentPassword))
            {
                _view.ShowError("No password generated yet. Please generate a password first.");
                return;
            }

            var key = _view.GetPasswordKey();
            var description = _view.GetPasswordDescription();

            // Create and save entry via model
            var entry = new PasswordEntry(key, _currentPassword, description);
            _model.SavePassword(entry);

            _view.ShowSuccess($"Password saved as '{key}'.");
            _currentPassword = null;
        }
        catch (Exception ex)
        {
            _view.ShowError(ex.Message);
        }
    }

    /// <summary>
    /// Handles viewing all saved passwords.
    /// </summary>
    private void HandleViewPasswords()
    {
        try
        {
            var entries = _model.GetAllPasswords();
            _view.ShowPasswordList(entries);
        }
        catch (Exception ex)
        {
            _view.ShowError(ex.Message);
        }
    }

    /// <summary>
    /// Handles password deletion flow.
    /// </summary>
    private void HandleDeletePassword()
    {
        try
        {
            var key = _view.GetPasswordKeyToDelete();
            var deleted = _model.DeletePassword(key);

            if (deleted)
            {
                _view.ShowSuccess($"Password '{key}' deleted.");
            }
            else
            {
                _view.ShowError($"Password '{key}' not found.");
            }
        }
        catch (Exception ex)
        {
            _view.ShowError(ex.Message);
        }
    }
}
