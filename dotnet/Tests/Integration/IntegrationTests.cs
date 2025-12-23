using Xunit;
using PasswordGenerator.Shared;
using Generator = PasswordGenerator.Shared.PasswordGenerator;

namespace PasswordGenerator.Tests;

/// <summary>
/// Integration tests demonstrating end-to-end flows.
/// Tests the complete generate → save → list → delete flow.
/// </summary>
public class IntegrationTests
{
    [Fact]
    public void FullFlow_GenerateAndSavePassword_Succeeds()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"it_{Guid.NewGuid()}.json");
        var storage = new JsonPasswordStorage(tempPath);
        var randomMock = RandomMockFactory.Create(Enumerable.Repeat((byte)0x10, 12).ToArray());
        var generator = new Generator(randomMock.Object);

        var config = new PasswordConfig(12, useSpecialChars: true);
        var password = generator.Generate(config);

        var entry = new PasswordEntry("mykey", password, "my password");
        storage.Save(entry);

        var retrieved = storage.Load("mykey");

        Assert.NotNull(retrieved);
        Assert.Equal(password, retrieved.Password);
        Assert.Equal("mykey", retrieved.Key);
        if (File.Exists(tempPath)) File.Delete(tempPath);
    }

    [Fact]
    public void FullFlow_SaveMultipleAndList_Succeeds()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"it_{Guid.NewGuid()}.json");
        var storage = new JsonPasswordStorage(tempPath);

        for (int i = 0; i < 5; i++)
        {
            var entry = new PasswordEntry($"key{i}", $"password{i}", $"description{i}");
            storage.Save(entry);
        }

        var allEntries = storage.LoadAll().ToList();

        Assert.Equal(5, allEntries.Count);
        Assert.All(allEntries, entry => Assert.NotNull(entry));

        if (File.Exists(tempPath)) File.Delete(tempPath);
    }

    [Fact]
    public void FullFlow_SaveAndDelete_Succeeds()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"it_{Guid.NewGuid()}.json");
        var storage = new JsonPasswordStorage(tempPath);
        var entry = new PasswordEntry("deletekey", "password", "to delete");
        storage.Save(entry);

        var deleted = storage.Delete("deletekey");

        Assert.True(deleted);
        Assert.Null(storage.Load("deletekey"));

        if (File.Exists(tempPath)) File.Delete(tempPath);
    }

    [Fact]
    public void FullFlow_GenerateWithDifferentSources_ProducesDifferentPasswords()
    {
        var randomSource1 = RandomMockFactory.Create(Enumerable.Repeat((byte)0x01, 20).ToArray());
        var randomSource2 = RandomMockFactory.Create(Enumerable.Repeat((byte)0xFF, 20).ToArray());
        var generator1 = new Generator(randomSource1.Object);
        var generator2 = new Generator(randomSource2.Object);

        var config = new PasswordConfig(20, useSpecialChars: false);

        var password1 = generator1.Generate(config);
        var password2 = generator2.Generate(config);

        Assert.NotEqual(password1, password2);
        Assert.Equal(20, password1.Length);
        Assert.Equal(20, password2.Length);
    }
}
