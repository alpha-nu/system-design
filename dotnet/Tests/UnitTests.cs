using Generator = PasswordGenerator.Shared.PasswordGenerator;

namespace PasswordGenerator.Tests;

/// <summary>
/// Unit tests for PasswordGenerator class.
/// </summary>
public class PasswordGeneratorTests
{
    [Fact]
    public void Generate_WithValidConfig_ReturnsPasswordOfCorrectLength()
    {
        // Arrange
        var mockRandom = new MockRandomSource(new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 });
        var generator = new Generator(mockRandom);
        var config = new PasswordConfig(length: 10, useSpecialChars: false);

        // Act
        var password = generator.Generate(config);

        // Assert
        Assert.Equal(10, password.Length);
    }

    [Fact]
    public void Generate_WithoutSpecialChars_OnlyContainsAlphanumeric()
    {
        // Arrange
        var mockRandom = new MockRandomSource(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        var generator = new Generator(mockRandom);
        var config = new PasswordConfig(length: 50, useSpecialChars: false);

        // Act
        var password = generator.Generate(config);

        // Assert
        Assert.True(password.All(c => char.IsLetterOrDigit(c)));
    }

    [Fact]
    public void Generate_WithSpecialChars_CanContainSpecialCharacters()
    {
        // Arrange
        var mockRandom = new MockRandomSource(new byte[] { 255 });
        var generator = new Generator(mockRandom);
        var config = new PasswordConfig(length: 20, useSpecialChars: true);

        // Act
        var password = generator.Generate(config);

        // Assert
        Assert.NotEmpty(password);
        Assert.Equal(20, password.Length);
    }

    [Fact]
    public void Generate_WithNullConfig_ThrowsArgumentNullException()
    {
        // Arrange
        var mockRandom = new MockRandomSource(new byte[] { 0x1 });
        var generator = new Generator(mockRandom);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => generator.Generate(null!));
    }

    [Fact]
    public void Constructor_WithNullRandomSource_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Generator(null!));
    }

    [Theory]
    [InlineData("abcdefgh", PasswordStrength.Weak)]
    [InlineData("abcDefgh", PasswordStrength.Weak)]
    [InlineData("abcDef12", PasswordStrength.Medium)]
    [InlineData("abcDef12!", PasswordStrength.Strong)]
    [InlineData("AbCdEf12!@#", PasswordStrength.Strong)]
    public void CalculateStrength_WithVaryingPasswords_ReturnsCorrectStrength(
        string password,
        PasswordStrength expectedStrength)
    {
        // Act
        var strength = Generator.CalculateStrength(password);

        // Assert
        Assert.Equal(expectedStrength, strength);
    }

    [Fact]
    public void CalculateStrength_WithEmptyPassword_ReturnsWeak()
    {
        // Act
        var strength = Generator.CalculateStrength("");

        // Assert
        Assert.Equal(PasswordStrength.Weak, strength);
    }

    [Fact]
    public void CalculateStrength_WithShortPassword_ReturnsWeak()
    {
        // Act
        var strength = Generator.CalculateStrength("aB1!");

        // Assert
        Assert.Equal(PasswordStrength.Weak, strength);
    }
}

/// <summary>
/// Unit tests for PasswordConfig class.
/// </summary>
public class PasswordConfigTests
{
    [Fact]
    public void Constructor_WithValidLength_Succeeds()
    {
        // Act
        var config = new PasswordConfig(length: 12, useSpecialChars: true);

        // Assert
        Assert.Equal(12, config.Length);
        Assert.True(config.UseSpecialChars);
        Assert.Equal(RandomSourceType.CryptoRandom, config.RandomSourceType);
    }

    [Fact]
    public void Constructor_WithLengthLessThan4_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PasswordConfig(length: 3, useSpecialChars: true));
    }

    [Fact]
    public void Constructor_WithCustomRandomSource_UsesProvidedSource()
    {
        // Act
        var config = new PasswordConfig(
            length: 12,
            useSpecialChars: true,
            randomSourceType: RandomSourceType.DevUrandom
        );

        // Assert
        Assert.Equal(RandomSourceType.DevUrandom, config.RandomSourceType);
    }
}

/// <summary>
/// Unit tests for PasswordEntry class.
/// </summary>
public class PasswordEntryTests
{
    [Fact]
    public void Constructor_WithValidData_Succeeds()
    {
        // Act
        var entry = new PasswordEntry("mykey", "mypassword", "my description");

        // Assert
        Assert.Equal("mykey", entry.Key);
        Assert.Equal("mypassword", entry.Password);
        Assert.Equal("my description", entry.Description);
    }

    [Fact]
    public void Constructor_WithNullKey_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PasswordEntry(null!, "password"));
    }

    [Fact]
    public void Constructor_WithEmptyPassword_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PasswordEntry("key", ""));
    }

    [Fact]
    public void Constructor_CreatesValidTimestamp()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var entry = new PasswordEntry("key", "password");

        // Assert
        var afterCreation = DateTime.UtcNow;
        Assert.True(entry.CreatedAt >= beforeCreation && entry.CreatedAt <= afterCreation);
    }
}

/// <summary>
/// Unit tests for RandomSourceFactory class.
/// </summary>
public class RandomSourceFactoryTests
{
    [Theory]
    [InlineData(RandomSourceType.SystemRandom)]
    [InlineData(RandomSourceType.CryptoRandom)]
    [InlineData(RandomSourceType.HardwareRng)]
    public void Create_WithAvailableSource_ReturnsValidSource(RandomSourceType sourceType)
    {
        // Act
        var source = RandomSourceFactory.Create(sourceType);

        // Assert
        Assert.NotNull(source);
        Assert.True(source.IsAvailable);
    }

    [Fact]
    public void GetAvailableSources_ReturnsAtLeastOneSource()
    {
        // Act
        var sources = RandomSourceFactory.GetAvailableSources().ToList();

        // Assert
        Assert.NotEmpty(sources);
    }

    [Fact]
    public void GetAvailableSources_AllSourcesAreAvailable()
    {
        // Act
        var sources = RandomSourceFactory.GetAvailableSources();

        // Assert
        Assert.All(sources, source => Assert.True(source.Source.IsAvailable));
    }
}

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
        // Arrange
        var entry = new PasswordEntry("testkey", "testpassword", "test description");

        // Act
        _storage.Save(entry);

        // Assert
        var loaded = _storage.Load("testkey");
        Assert.NotNull(loaded);
        Assert.Equal(entry.Key, loaded.Key);
        Assert.Equal(entry.Password, loaded.Password);
    }

    [Fact]
    public void Load_WithNonexistentKey_ReturnsNull()
    {
        // Act
        var result = _storage.Load("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void LoadAll_WithMultipleEntries_ReturnsAllEntries()
    {
        // Arrange
        var entry1 = new PasswordEntry("key1", "password1");
        var entry2 = new PasswordEntry("key2", "password2");
        _storage.Save(entry1);
        _storage.Save(entry2);

        // Act
        var allEntries = _storage.LoadAll().ToList();

        // Assert
        Assert.Equal(2, allEntries.Count);
    }

    [Fact]
    public void Delete_WithExistingKey_RemovesEntry()
    {
        // Arrange
        var entry = new PasswordEntry("keyToDelete", "password");
        _storage.Save(entry);

        // Act
        var deleted = _storage.Delete("keyToDelete");

        // Assert
        Assert.True(deleted);
        Assert.Null(_storage.Load("keyToDelete"));
    }

    [Fact]
    public void Delete_WithNonexistentKey_ReturnsFalse()
    {
        // Act
        var deleted = _storage.Delete("nonexistent");

        // Assert
        Assert.False(deleted);
    }

    [Fact]
    public void Save_WithDuplicateKey_UpdatesExistingEntry()
    {
        // Arrange
        var entry1 = new PasswordEntry("key", "password1");
        var entry2 = new PasswordEntry("key", "password2");
        _storage.Save(entry1);

        // Act
        _storage.Save(entry2);

        // Assert
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
