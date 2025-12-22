using Generator = PasswordGenerator.Shared.PasswordGenerator;

namespace PasswordGenerator.MVC;

/// <summary>
/// MVC Model encapsulating password generation and storage business logic.
/// Pure business logic with no awareness of View or Controller.
/// </summary>
public sealed class PasswordModel
{
    private readonly Generator _generator;
    private readonly IPasswordStorage _storage;

    /// <summary>
    /// Initializes a new instance of the PasswordModel class.
    /// </summary>
    /// <param name="generator">Password generator instance.</param>
    /// <param name="storage">Password storage instance.</param>
    public PasswordModel(Generator generator, IPasswordStorage storage)
    {
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    /// <summary>
    /// Generates a password according to the specified configuration.
    /// </summary>
    /// <param name="config">Password configuration.</param>
    /// <returns>The generated password.</returns>
    public string GeneratePassword(PasswordConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        return _generator.Generate(config);
    }

    /// <summary>
    /// Saves a password entry to storage.
    /// </summary>
    /// <param name="entry">Password entry to save.</param>
    public void SavePassword(PasswordEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _storage.Save(entry);
    }

    /// <summary>
    /// Retrieves all stored password entries.
    /// </summary>
    /// <returns>Collection of password entries.</returns>
    public IEnumerable<PasswordEntry> GetAllPasswords()
    {
        return _storage.LoadAll();
    }

    /// <summary>
    /// Deletes a password entry by key.
    /// </summary>
    /// <param name="key">Password entry key.</param>
    /// <returns>True if deleted, false if not found.</returns>
    public bool DeletePassword(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        return _storage.Delete(key);
    }

    /// <summary>
    /// Calculates the strength of a password.
    /// </summary>
    /// <param name="password">The password to evaluate.</param>
    /// <returns>The password strength.</returns>
    public PasswordStrength GetPasswordStrength(string password)
    {
        return Generator.CalculateStrength(password);
    }

    /// <summary>
    /// Gets all available random sources.
    /// </summary>
    /// <returns>Collection of available random sources with their types.</returns>
    public IEnumerable<(RandomSourceType Type, string Name)> GetAvailableRandomSources()
    {
        return RandomSourceFactory.GetAvailableSources()
            .Select(s => (s.Type, s.Source.Name));
    }
}
