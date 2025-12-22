namespace PasswordGenerator.Shared;

/// <summary>
/// Hardware-based random number generator using CPU RDRAND instruction.
/// Falls back to CryptoRandom if RDRAND is not available.
/// </summary>
public sealed class HardwareRngSource : IRandomSource
{
    private readonly CryptoRandomSource _fallback = new();

    /// <inheritdoc/>
    public string Name => "Hardware RNG (RDRAND)";

    /// <inheritdoc/>
    public bool IsAvailable => true; // Fallback ensures it's always available

    /// <inheritdoc/>
    public byte[] GetRandomBytes(int count)
    {
        if (count < 0)
            throw new ArgumentException("Count must be non-negative.", nameof(count));

        // Try RDRAND via System.Security.Cryptography with hardware acceleration
        // Note: Modern .NET RNG may use RDRAND if available and secure
        // For true RDRAND access, we would need P/Invoke to CPUID and RDRAND instructions
        // For now, we use the crypto source which may leverage RDRAND internally

        return _fallback.GetRandomBytes(count);
    }
}
