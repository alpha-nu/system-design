using System.ComponentModel;
using Generator = PasswordGenerator.Shared.PasswordGenerator;

namespace PasswordGenerator.MVVM;

/// <summary>
/// MVVM ViewModel for password generation and storage.
/// Exposes observable properties and commands for the View to bind to.
/// </summary>
public sealed class PasswordViewModel : INotifyPropertyChanged
{
    private readonly Generator _generator;
    private readonly IPasswordStorage _storage;

    private string _currentPassword = "";
    private PasswordStrength _currentPasswordStrength = PasswordStrength.Weak;
    private RandomSourceType _selectedRandomSource = RandomSourceType.CryptoRandom;
    private int _passwordLength = 12;
    private bool _useSpecialChars = true;

    public event EventHandler<PropertyChangedEventArgs>? PropertyChanged;

    /// <summary>Initializes a new instance of the PasswordViewModel class.</summary>
    public PasswordViewModel(Generator generator, IPasswordStorage storage)
    {
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));

        // Initialize commands
        GeneratePasswordCommand = new RelayCommand(_ => HandleGeneratePassword());
        SavePasswordCommand = new RelayCommand(_ => HandleSavePassword(), _ => !string.IsNullOrEmpty(CurrentPassword));
        ViewAllPasswordsCommand = new RelayCommand(_ => HandleViewAllPasswords());
        DeletePasswordCommand = new RelayCommand(_ => HandleDeletePassword());
    }

    #region Observable Properties

    /// <summary>Gets or sets the current generated password.</summary>
    public string CurrentPassword
    {
        get => _currentPassword;
        set
        {
            if (_currentPassword != value)
            {
                _currentPassword = value;
                OnPropertyChanged(nameof(CurrentPassword));
                ((RelayCommand)SavePasswordCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>Gets or sets the current password's strength.</summary>
    public PasswordStrength CurrentPasswordStrength
    {
        get => _currentPasswordStrength;
        set
        {
            if (_currentPasswordStrength != value)
            {
                _currentPasswordStrength = value;
                OnPropertyChanged(nameof(CurrentPasswordStrength));
            }
        }
    }

    /// <summary>Gets or sets the desired password length.</summary>
    public int PasswordLength
    {
        get => _passwordLength;
        set
        {
            if (_passwordLength != value && value >= 4)
            {
                _passwordLength = value;
                OnPropertyChanged(nameof(PasswordLength));
            }
        }
    }

    /// <summary>Gets or sets whether to use special characters.</summary>
    public bool UseSpecialChars
    {
        get => _useSpecialChars;
        set
        {
            if (_useSpecialChars != value)
            {
                _useSpecialChars = value;
                OnPropertyChanged(nameof(UseSpecialChars));
            }
        }
    }

    /// <summary>Gets or sets the selected random source.</summary>
    public RandomSourceType SelectedRandomSource
    {
        get => _selectedRandomSource;
        set
        {
            if (_selectedRandomSource != value)
            {
                _selectedRandomSource = value;
                OnPropertyChanged(nameof(SelectedRandomSource));
            }
        }
    }

    /// <summary>Gets available random sources.</summary>
    public IEnumerable<(RandomSourceType Type, string Name)> AvailableRandomSources =>
        RandomSourceFactory.GetAvailableSources().Select(s => (s.Type, s.Source.Name));

    /// <summary>Gets all saved password entries.</summary>
    public IEnumerable<PasswordEntry> SavedPasswords => _storage.LoadAll();

    #endregion

    #region Commands

    /// <summary>Command to generate a new password.</summary>
    public ICommand GeneratePasswordCommand { get; }

    /// <summary>Command to save the current password.</summary>
    public ICommand SavePasswordCommand { get; }

    /// <summary>Command to view all passwords.</summary>
    public ICommand ViewAllPasswordsCommand { get; }

    /// <summary>Command to delete a password.</summary>
    public ICommand DeletePasswordCommand { get; }

    #endregion

    #region Command Handlers

    private void HandleGeneratePassword()
    {
        try
        {
            var config = new PasswordConfig(PasswordLength, UseSpecialChars, SelectedRandomSource);
            var randomSource = RandomSourceFactory.Create(SelectedRandomSource);
            var generator = new Generator(randomSource);

            CurrentPassword = generator.Generate(config);
            CurrentPasswordStrength = Generator.CalculateStrength(CurrentPassword);
        }
        catch (Exception ex)
        {
            CurrentPassword = $"Error: {ex.Message}";
        }
    }

    private void HandleSavePassword()
    {
        try
        {
            if (string.IsNullOrEmpty(CurrentPassword))
                throw new InvalidOperationException("No password to save");

            // In a real application, we'd prompt for key/description here
            string key = $"Password_{DateTime.Now:yyyyMMddHHmmss}";
            var entry = new PasswordEntry(key, CurrentPassword, "Generated password");
            _storage.Save(entry);

            CurrentPassword = "";
            OnPropertyChanged(nameof(SavedPasswords));
        }
        catch (Exception ex)
        {
            CurrentPassword = $"Error: {ex.Message}";
        }
    }

    private void HandleViewAllPasswords()
    {
        OnPropertyChanged(nameof(SavedPasswords));
    }

    private void HandleDeletePassword()
    {
        // In a real application, we'd select which one to delete
        OnPropertyChanged(nameof(SavedPasswords));
    }

    #endregion

    /// <summary>Raises the PropertyChanged event.</summary>
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
