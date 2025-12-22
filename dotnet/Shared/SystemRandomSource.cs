namespace PasswordGenerator.Shared;

/// <summary>
/// System default random number generator.
/// Uses platform-dependent random source (may not be cryptographically secure).
/// </summary>
public sealed class SystemRandomSource : IRandomSource
{
    private readonly Random _random = new();

    /// <inheritdoc/>
    public string Name => "System Random";

    /// <inheritdoc/>
    public bool IsAvailable => true;

    /// <inheritdoc/>
    public byte[] GetRandomBytes(int count)
    {
        if (count < 0)
            throw new ArgumentException("Count must be non-negative.", nameof(count));

        var bytes = new byte[count];
        _random.NextBytes(bytes);
        return bytes;
    }
}
