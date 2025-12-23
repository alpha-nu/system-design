using Xunit;
using PasswordGenerator.Shared;

namespace PasswordGenerator.Tests;

/// <summary>
/// Unit tests for PasswordEntry class.
/// </summary>
public class PasswordEntryTests
{
    [Fact]
    public void Constructor_WithValidData_Succeeds()
    {
        var entry = new PasswordEntry("mykey", "mypassword", "my description");

        Assert.Equal("mykey", entry.Key);
        Assert.Equal("mypassword", entry.Password);
        Assert.Equal("my description", entry.Description);
    }

    [Fact]
    public void Constructor_WithNullKey_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new PasswordEntry(null!, "password"));
    }

    [Fact]
    public void Constructor_WithEmptyPassword_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new PasswordEntry("key", ""));
    }

    [Fact]
    public void Constructor_CreatesValidTimestamp()
    {
        var beforeCreation = DateTime.UtcNow;

        var entry = new PasswordEntry("key", "password");

        var afterCreation = DateTime.UtcNow;
        Assert.True(entry.CreatedAt >= beforeCreation && entry.CreatedAt <= afterCreation);
    }
}
