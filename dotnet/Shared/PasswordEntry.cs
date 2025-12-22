namespace PasswordGenerator.Shared;

/// <summary>
/// Represents a stored password entry with metadata.
/// </summary>
public sealed class PasswordEntry
{
    /// <summary>
    /// Gets the unique identifier/name for this password.
    /// </summary>
    public string Key { get; }
    
    /// <summary>
    /// Gets the actual password value.
    /// </summary>
    public string Password { get; }
    
    /// <summary>
    /// Gets the description/notes for this password.
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    /// Gets the timestamp when this entry was created.
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Initializes a new instance of the PasswordEntry class.
    /// </summary>
    /// <param name="key">Unique identifier for the password.</param>
    /// <param name="password">The password value.</param>
    /// <param name="description">Optional description.</param>
    /// <param name="createdAt">Timestamp of creation (defaults to now).</param>
    /// <exception cref="ArgumentException">Thrown when key or password is null/empty.</exception>
    public PasswordEntry(string key, string password, string description = "", DateTime? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        Key = key;
        Password = password;
        Description = description;
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }
}
