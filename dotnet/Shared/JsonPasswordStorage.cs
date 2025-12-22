using System.Text.Json;

namespace PasswordGenerator.Shared;

/// <summary>
/// JSON file-based password storage implementation.
/// Stores password entries in a JSON file in the user's home directory.
/// </summary>
public sealed class JsonPasswordStorage : IPasswordStorage
{
    private readonly string _storagePath;
    private readonly object _lockObject = new();

    /// <summary>
    /// Initializes a new instance of the JsonPasswordStorage class.
    /// </summary>
    /// <param name="storagePath">Optional custom storage path. Defaults to ~/.password-generator/passwords.json</param>
    public JsonPasswordStorage(string? storagePath = null)
    {
        _storagePath = storagePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".password-generator",
            "passwords.json"
        );

        // Ensure directory exists
        var directory = Path.GetDirectoryName(_storagePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    /// <inheritdoc/>
    public void Save(PasswordEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        lock (_lockObject)
        {
            try
            {
                var entries = LoadAllInternal().ToList();
                // Remove existing entry with same key
                entries.RemoveAll(e => e.Key == entry.Key);
                // Add new entry
                entries.Add(entry);

                // Serialize to JSON
                var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_storagePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save password to {_storagePath}.", ex);
            }
        }
    }

    /// <inheritdoc/>
    public PasswordEntry? Load(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        lock (_lockObject)
        {
            return LoadAllInternal().FirstOrDefault(e => e.Key == key);
        }
    }

    /// <inheritdoc/>
    public IEnumerable<PasswordEntry> LoadAll()
    {
        lock (_lockObject)
        {
            return LoadAllInternal().ToList();
        }
    }

    /// <inheritdoc/>
    public bool Delete(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        lock (_lockObject)
        {
            try
            {
                var entries = LoadAllInternal().ToList();
                var initialCount = entries.Count;
                entries.RemoveAll(e => e.Key == key);

                if (entries.Count == initialCount)
                    return false; // Nothing was deleted

                if (entries.Count == 0)
                {
                    // Delete the file if empty
                    if (File.Exists(_storagePath))
                        File.Delete(_storagePath);
                }
                else
                {
                    var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_storagePath, json);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete password from {_storagePath}.", ex);
            }
        }
    }

    /// <summary>
    /// Loads all entries from the storage file (internal, not locked).
    /// </summary>
    private IEnumerable<PasswordEntry> LoadAllInternal()
    {
        if (!File.Exists(_storagePath))
            return Enumerable.Empty<PasswordEntry>();

        try
        {
            var json = File.ReadAllText(_storagePath);
            if (string.IsNullOrWhiteSpace(json))
                return Enumerable.Empty<PasswordEntry>();

            var entries = JsonSerializer.Deserialize<List<PasswordEntryDto>>(json);
            return entries?.ConvertAll(dto => dto.ToPasswordEntry()) ?? Enumerable.Empty<PasswordEntry>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load passwords from {_storagePath}.", ex);
        }
    }

    /// <summary>
    /// DTO for JSON serialization (includes all fields).
    /// </summary>
    private sealed class PasswordEntryDto
    {
        public string Key { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public PasswordEntry ToPasswordEntry() =>
            new(Key, Password, Description, CreatedAt);
    }
}
