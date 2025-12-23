using Xunit;
using PasswordGenerator.Shared;

namespace PasswordGenerator.Tests;

/// <summary>
/// Unit tests for JsonPasswordStorage class.
/// </summary>
public class JsonPasswordStorageTests : IDisposable
{
    private readonly string _tempFilePath;
    private readonly JsonPasswordStorage _storage;

    public JsonPasswordStorageTests()
    {
        _tempFilePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
        _storage = new JsonPasswordStorage(_tempFilePath);
    }

    [Fact]
    public void Save_WithValidEntry_PersistsData()
    {
        var entry = new PasswordEntry("testkey", "testpassword", "test description");

        _storage.Save(entry);

        var loaded = _storage.Load("testkey");
        Assert.NotNull(loaded);
        Assert.Equal(entry.Key, loaded.Key);
        Assert.Equal(entry.Password, loaded.Password);
    }

    [Fact]
    public void Load_WithNonexistentKey_ReturnsNull()
    {
        var result = _storage.Load("nonexistent");
        Assert.Null(result);
    }

    [Fact]
    public void LoadAll_WithMultipleEntries_ReturnsAllEntries()
    {
        var entry1 = new PasswordEntry("key1", "password1");
        var entry2 = new PasswordEntry("key2", "password2");
        _storage.Save(entry1);
        _storage.Save(entry2);

        var allEntries = _storage.LoadAll().ToList();

        Assert.Equal(2, allEntries.Count);
    }

    [Fact]
    public void Delete_WithExistingKey_RemovesEntry()
    {
        var entry = new PasswordEntry("keyToDelete", "password");
        _storage.Save(entry);

        var deleted = _storage.Delete("keyToDelete");

        Assert.True(deleted);
        Assert.Null(_storage.Load("keyToDelete"));
    }

    [Fact]
    public void Delete_WithNonexistentKey_ReturnsFalse()
    {
        var deleted = _storage.Delete("nonexistent");

        Assert.False(deleted);
    }

    [Fact]
    public void Save_WithDuplicateKey_UpdatesExistingEntry()
    {
        var entry1 = new PasswordEntry("key", "password1");
        var entry2 = new PasswordEntry("key", "password2");
        _storage.Save(entry1);

        _storage.Save(entry2);

        var loaded = _storage.Load("key");
        Assert.Equal("password2", loaded?.Password);
        Assert.Single(_storage.LoadAll());
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }

        var directory = Path.GetDirectoryName(_tempFilePath);
        if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
        {
            try
            {
                Directory.Delete(directory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
