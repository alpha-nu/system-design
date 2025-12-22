namespace PasswordGenerator.Shared;

/// <summary>
/// Interface for password persistence storage.
/// </summary>
public interface IPasswordStorage
{
    /// <summary>
    /// Saves a password entry.
    /// </summary>
    /// <param name="entry">Password entry to save.</param>
    /// <exception cref="InvalidOperationException">Thrown if save fails.</exception>
    void Save(PasswordEntry entry);

    /// <summary>
    /// Loads a password entry by key.
    /// </summary>
    /// <param name="key">The password entry key.</param>
    /// <returns>The password entry, or null if not found.</returns>
    PasswordEntry? Load(string key);

    /// <summary>
    /// Retrieves all stored password entries.
    /// </summary>
    /// <returns>Collection of all password entries.</returns>
    IEnumerable<PasswordEntry> LoadAll();

    /// <summary>
    /// Deletes a password entry by key.
    /// </summary>
    /// <param name="key">The password entry key.</param>
    /// <returns>True if deleted, false if not found.</returns>
    bool Delete(string key);
}
