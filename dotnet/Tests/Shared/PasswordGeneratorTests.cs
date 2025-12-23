using Xunit;
using PasswordGenerator.Shared;
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
        var randomMock = RandomMockFactory.Create(Enumerable.Repeat((byte)1, 10).ToArray());
        var generator = new Generator(randomMock.Object);
        var config = new PasswordConfig(length: 10, useSpecialChars: false);

        var password = generator.Generate(config);

        Assert.Equal(10, password.Length);
    }

    [Fact]
    public void Generate_WithoutSpecialChars_OnlyContainsAlphanumeric()
    {
        var randomMock = RandomMockFactory.Create(Enumerable.Range(0, 50).Select(i => (byte)i).ToArray());
        var generator = new Generator(randomMock.Object);
        var config = new PasswordConfig(length: 50, useSpecialChars: false);

        var password = generator.Generate(config);

        Assert.True(password.All(c => char.IsLetterOrDigit(c)));
    }

    [Fact]
    public void Generate_WithSpecialChars_CanContainSpecialCharacters()
    {
        var randomMock = RandomMockFactory.Create(Enumerable.Repeat((byte)255, 20).ToArray());
        var generator = new Generator(randomMock.Object);
        var config = new PasswordConfig(length: 20, useSpecialChars: true);

        var password = generator.Generate(config);

        Assert.NotEmpty(password);
        Assert.Equal(20, password.Length);
    }

    [Fact]
    public void Generate_WithNullConfig_ThrowsArgumentNullException()
    {
        var randomMock = RandomMockFactory.Create(new byte[] { 0x1 });
        var generator = new Generator(randomMock.Object);

        Assert.Throws<ArgumentNullException>(() => generator.Generate(null!));
    }

    [Fact]
    public void Constructor_WithNullRandomSource_ThrowsArgumentNullException()
    {
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
        var strength = Generator.CalculateStrength(password);
        Assert.Equal(expectedStrength, strength);
    }

    [Fact]
    public void CalculateStrength_WithEmptyPassword_ReturnsWeak()
    {
        var strength = Generator.CalculateStrength("");
        Assert.Equal(PasswordStrength.Weak, strength);
    }

    [Fact]
    public void CalculateStrength_WithShortPassword_ReturnsWeak()
    {
        var strength = Generator.CalculateStrength("aB1!");
        Assert.Equal(PasswordStrength.Weak, strength);
    }
}
