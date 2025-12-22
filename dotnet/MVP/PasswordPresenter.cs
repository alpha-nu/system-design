using PasswordGenerator.Shared;
using Generator = PasswordGenerator.Shared.PasswordGenerator;

namespace PasswordGenerator.MVP;

/// <summary>
/// MVP Presenter - mediates between the Passive View and Model.
/// Subscribes to View events and updates the View based on Model responses.
/// The Presenter has NO direct dependency on concrete View, only on IPasswordView interface.
/// </summary>
public sealed class PasswordPresenter
{
    private readonly IPasswordView _view;
    private readonly Generator _generator;
    private readonly IPasswordStorage _storage;
    private string? _currentPassword;
    private RandomSourceType _currentRandomSource = RandomSourceType.CryptoRandom;

    /// <summary>
    /// Initializes a new instance of the PasswordPresenter class.
    /// </summary>
    /// <param name="view">The password view interface.</param>
    /// <param name="generator">Password generator.</param>
    /// <param name="storage">Password storage.</param>
    public PasswordPresenter(IPasswordView view, Generator generator, IPasswordStorage storage)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));

        // Subscribe to view events
        _view.OnGenerateRequested += HandleGeneratePassword;
        _view.OnSaveRequested += HandleSavePassword;
        _view.OnViewAllRequested += HandleViewPasswords;
        _view.OnDeleteRequested += HandleDeletePassword;
        _view.OnExitRequested += HandleExit;
    }

    /// <summary>
    /// Runs the presentation loop.
    /// </summary>
    public void Run()
    {
        while (true)
        {
            try
            {
                // Update available random sources
                var sources = RandomSourceFactory.GetAvailableSources()
                    .Select(s => (s.Type, s.Source.Name));
                _view.SetAvailableRandomSources(sources);

                _view.ShowMainMenu();
            }
            catch (Exception ex)
            {
                _view.ShowError($"An unexpected error occurred: {ex.Message}");
            }
        }
    }

    private void HandleGeneratePassword(object? sender, EventArgs e)
    {
        try
        {
            var sources = RandomSourceFactory.GetAvailableSources().ToList();
            if (_view.SelectedRandomSourceIndex >= 0 && _view.SelectedRandomSourceIndex < sources.Count)
            {
                _currentRandomSource = sources[_view.SelectedRandomSourceIndex].Type;
            }

            var config = new PasswordConfig(_view.PasswordLength, _view.UseSpecialChars, _currentRandomSource);
            var randomSource = RandomSourceFactory.Create(_currentRandomSource);
            var generator = new Generator(randomSource);

            _currentPassword = generator.Generate(config);
            var strength = Generator.CalculateStrength(_currentPassword);

            _view.DisplayGeneratedPassword(_currentPassword, strength);
        }
        catch (Exception ex)
        {
            _view.ShowError(ex.Message);
        }
    }

    private void HandleSavePassword(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(_currentPassword))
            {
                _view.ShowError("No password generated yet. Please generate a password first.");
                return;
            }

            var entry = new PasswordEntry(_view.PasswordKey, _currentPassword, _view.PasswordDescription);
            _storage.Save(entry);

            _view.ShowSuccess($"Password saved as '{_view.PasswordKey}'.");
            _currentPassword = null;
        }
        catch (Exception ex)
        {
            _view.ShowError(ex.Message);
        }
    }

    private void HandleViewPasswords(object? sender, EventArgs e)
    {
        try
        {
            var entries = _storage.LoadAll();
            _view.DisplayPasswordList(entries);
        }
        catch (Exception ex)
        {
            _view.ShowError(ex.Message);
        }
    }

    private void HandleDeletePassword(object? sender, EventArgs e)
    {
        try
        {
            var deleted = _storage.Delete(_view.DeletePasswordKey);
            if (deleted)
            {
                _view.ShowSuccess($"Password '{_view.DeletePasswordKey}' deleted.");
            }
            else
            {
                _view.ShowError($"Password '{_view.DeletePasswordKey}' not found.");
            }
        }
        catch (Exception ex)
        {
            _view.ShowError(ex.Message);
        }
    }

    private void HandleExit(object? sender, EventArgs e)
    {
        Console.WriteLine("\nGoodbye!");
        Environment.Exit(0);
    }
}
