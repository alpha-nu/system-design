using Xunit;
using PasswordGenerator.Shared;

namespace PasswordGenerator.Tests;

/// <summary>
/// Unit tests for PasswordConfig class.
/// </summary>
public class PasswordConfigTests
{
    [Fact]
    public void Constructor_WithValidLength_Succeeds()
    {
        var config = new PasswordConfig(length: 12, useSpecialChars: true);

        Assert.Equal(12, config.Length);
        Assert.True(config.UseSpecialChars);
        Assert.Equal(RandomSourceType.CryptoRandom, config.RandomSourceType);
    }

    [Fact]
    public void Constructor_WithLengthLessThan4_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new PasswordConfig(length: 3, useSpecialChars: true));
    }

    [Fact]
    public void Constructor_WithCustomRandomSource_UsesProvidedSource()
    {
        var config = new PasswordConfig(
            length: 12,
            useSpecialChars: true,
            randomSourceType: RandomSourceType.DevUrandom
        );

        Assert.Equal(RandomSourceType.DevUrandom, config.RandomSourceType);
    }
}
