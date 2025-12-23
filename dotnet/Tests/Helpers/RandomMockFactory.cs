using PasswordGenerator.Shared;
using Moq;

namespace PasswordGenerator.Tests;

/// <summary>
/// Factory helpers for creating IRandomSource mocks with deterministic sequences.
/// </summary>
public static class RandomMockFactory
{
    public static Mock<IRandomSource> Create(byte[] sequence, string name = "Mock Random")
    {
        if (sequence is null || sequence.Length == 0)
            throw new ArgumentException("Sequence cannot be null or empty.", nameof(sequence));

        var mock = new Mock<IRandomSource>();
        mock.SetupGet(r => r.Name).Returns(name);
        mock.SetupGet(r => r.IsAvailable).Returns(true);
        mock.Setup(r => r.GetRandomBytes(It.IsAny<int>()))
            .Returns(sequence);
        return mock;
    }
}
