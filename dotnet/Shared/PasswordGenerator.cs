namespace PasswordGenerator.Shared;

/// <summary>
/// Pure business logic for generating passwords.
/// This class has no external dependencies and focuses solely on password generation.
/// </summary>
public sealed class PasswordGenerator
{
    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string NumericChars = "0123456789";
    private const string SpecialChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";

    private readonly IRandomSource _randomSource;

    /// <summary>
    /// Initializes a new instance of the PasswordGenerator class.
    /// </summary>
    /// <param name="randomSource">Source of randomness for password generation.</param>
    /// <exception cref="ArgumentNullException">Thrown when randomSource is null.</exception>
    public PasswordGenerator(IRandomSource randomSource)
    {
        _randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
    }

    /// <summary>
    /// Generates a password according to the specified configuration.
    /// </summary>
    /// <param name="config">Password configuration.</param>
    /// <returns>The generated password.</returns>
    /// <exception cref="ArgumentNullException">Thrown when config is null.</exception>
    public string Generate(PasswordConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var charPool = BuildCharacterPool(config.UseSpecialChars);
        var password = new char[config.Length];

        // Generate random indices
        var randomBytes = _randomSource.GetRandomBytes(config.Length);

        for (int i = 0; i < config.Length; i++)
        {
            var index = randomBytes[i] % charPool.Length;
            password[i] = charPool[index];
        }

        return new string(password);
    }

    /// <summary>
    /// Calculates the strength of a password.
    /// </summary>
    /// <param name="password">The password to evaluate.</param>
    /// <returns>The password strength level.</returns>
    public static PasswordStrength CalculateStrength(string password)
    {
        if (string.IsNullOrEmpty(password))
            return PasswordStrength.Weak;

        bool hasLower = password.Any(c => char.IsLower(c));
        bool hasUpper = password.Any(c => char.IsUpper(c));
        bool hasDigit = password.Any(c => char.IsDigit(c));
        bool hasSpecial = password.Any(c => SpecialChars.Contains(c));

        int strength = 0;
        if (hasLower) strength++;
        if (hasUpper) strength++;
        if (hasDigit) strength++;
        if (hasSpecial) strength++;

        if (password.Length < 8)
            return PasswordStrength.Weak;

        if (strength <= 2)
            return PasswordStrength.Weak;

        if (strength == 3)
            return PasswordStrength.Medium;

        return PasswordStrength.Strong;
    }

    /// <summary>
    /// Builds the character pool based on configuration.
    /// </summary>
    private static string BuildCharacterPool(bool useSpecialChars)
    {
        var pool = LowercaseChars + UppercaseChars + NumericChars;
        if (useSpecialChars)
            pool += SpecialChars;
        return pool;
    }
}
