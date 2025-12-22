namespace PasswordGenerator.Shared;

/// <summary>
/// Enumeration of available random number sources for password generation.
/// </summary>
public enum RandomSourceType
{
    /// <summary>System random (default, platform-dependent).</summary>
    SystemRandom,
    
    /// <summary>Cryptographic random using .NET's RNG.</summary>
    CryptoRandom,
    
    /// <summary>Unix /dev/urandom (Unix-only).</summary>
    DevUrandom,
    
    /// <summary>Hardware-based RNG (CPU RDRAND if available).</summary>
    HardwareRng
}
