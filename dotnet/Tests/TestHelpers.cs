using PasswordGenerator.Shared;

namespace PasswordGenerator.Tests;

/// <summary>
/// Mock implementation of IPasswordStorage for testing.
/// Stores passwords in memory without any file I/O.
/// </summary>
public sealed class InMemoryPasswordStorage : IPasswordStorage
{
    private readonly Dictionary<string, PasswordEntry> _storage = new();

    public void Save(PasswordEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _storage[entry.Key] = entry;
    }

    public PasswordEntry? Load(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        _storage.TryGetValue(key, out var entry);
        return entry;
    }

    public IEnumerable<PasswordEntry> LoadAll()
    {
        return _storage.Values.ToList();
    }

    public bool Delete(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        return _storage.Remove(key);
    }
}

/// <summary>
/// Mock implementation of IRandomSource for testing.
/// Returns predetermined bytes for consistent testing.
/// </summary>
public sealed class MockRandomSource : IRandomSource
{
    private readonly byte[] _predeterminedBytes;
    private int _callCount;

    public string Name => "Mock Random";
    public bool IsAvailable => true;

    public MockRandomSource(byte[] predeterminedBytes)
    {
        _predeterminedBytes = predeterminedBytes ?? throw new ArgumentNullException(nameof(predeterminedBytes));
    }

    public byte[] GetRandomBytes(int count)
    {
        if (count < 0)
            throw new ArgumentException("Count must be non-negative.", nameof(count));

        _callCount++;
        var result = new byte[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = _predeterminedBytes[i % _predeterminedBytes.Length];
        }
        return result;
    }

    public int CallCount => _callCount;
}
