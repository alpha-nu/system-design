namespace PasswordGenerator.Shared;

/// <summary>
/// Interface for random number generation sources.
/// </summary>
public interface IRandomSource
{
    /// <summary>
    /// Gets the name of this random source.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets a value indicating whether this source is available on the current platform.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Generates cryptographically secure random bytes.
    /// </summary>
    /// <param name="count">Number of bytes to generate.</param>
    /// <returns>Array of random bytes.</returns>
    /// <exception cref="InvalidOperationException">Thrown when source is not available.</exception>
    byte[] GetRandomBytes(int count);
}
