using Xunit;
using PasswordGenerator.Shared;

namespace PasswordGenerator.Tests;

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
        var source = RandomSourceFactory.Create(sourceType);

        Assert.NotNull(source);
        Assert.True(source.IsAvailable);
    }

    [Fact]
    public void GetAvailableSources_ReturnsAtLeastOneSource()
    {
        var sources = RandomSourceFactory.GetAvailableSources().ToList();

        Assert.NotEmpty(sources);
    }

    [Fact]
    public void GetAvailableSources_AllSourcesAreAvailable()
    {
        var sources = RandomSourceFactory.GetAvailableSources();

        Assert.All(sources, source => Assert.True(source.Source.IsAvailable));
    }
}
