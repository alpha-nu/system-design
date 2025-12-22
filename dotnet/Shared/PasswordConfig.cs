namespace PasswordGenerator.Shared;

/// <summary>
/// Immutable configuration for password generation.
/// </summary>
public sealed class PasswordConfig
{
    /// <summary>
    /// Gets the length of the password to generate.
    /// </summary>
    public int Length { get; }
    
    /// <summary>
    /// Gets a value indicating whether special characters should be included.
    /// </summary>
    public bool UseSpecialChars { get; }
    
    /// <summary>
    /// Gets the source of randomness to use.
    /// </summary>
    public RandomSourceType RandomSourceType { get; }

    /// <summary>
    /// Initializes a new instance of the PasswordConfig class.
    /// </summary>
    /// <param name="length">Password length (minimum 4).</param>
    /// <param name="useSpecialChars">Whether to include special characters.</param>
    /// <param name="randomSourceType">Source of randomness.</param>
    /// <exception cref="ArgumentException">Thrown when length is less than 4.</exception>
    public PasswordConfig(int length, bool useSpecialChars, RandomSourceType randomSourceType = RandomSourceType.CryptoRandom)
    {
        if (length < 4)
            throw new ArgumentException("Password length must be at least 4 characters.", nameof(length));

        Length = length;
        UseSpecialChars = useSpecialChars;
        RandomSourceType = randomSourceType;
    }
}
