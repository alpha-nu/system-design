using System.Security.Cryptography;

namespace PasswordGenerator.Shared;

/// <summary>
/// Cryptographically secure random number generator using .NET's RNG.
/// </summary>
public sealed class CryptoRandomSource : IRandomSource
{
    /// <inheritdoc/>
    public string Name => "Crypto Random (.NET)";

    /// <inheritdoc/>
    public bool IsAvailable => true;

    /// <inheritdoc/>
    public byte[] GetRandomBytes(int count)
    {
        if (count < 0)
            throw new ArgumentException("Count must be non-negative.", nameof(count));

        var bytes = new byte[count];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return bytes;
    }
}
