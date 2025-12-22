namespace PasswordGenerator.Shared;

/// <summary>
/// Factory for creating random source instances by type.
/// </summary>
public static class RandomSourceFactory
{
    /// <summary>
    /// Creates a random source instance based on the specified type.
    /// </summary>
    /// <param name="type">The desired random source type.</param>
    /// <returns>An instance of the requested random source.</returns>
    /// <exception cref="InvalidOperationException">Thrown if source is not available on current platform.</exception>
    public static IRandomSource Create(RandomSourceType type)
    {
        IRandomSource source = type switch
        {
            RandomSourceType.SystemRandom => new SystemRandomSource(),
            RandomSourceType.CryptoRandom => new CryptoRandomSource(),
            RandomSourceType.DevUrandom => new DevUrandomSource(),
            RandomSourceType.HardwareRng => new HardwareRngSource(),
            _ => throw new ArgumentException($"Unknown random source type: {type}", nameof(type))
        };

        if (!source.IsAvailable)
            throw new InvalidOperationException($"Random source '{source.Name}' is not available on this platform.");

        return source;
    }

    /// <summary>
    /// Gets all available random sources on the current platform.
    /// </summary>
    /// <returns>Collection of available random sources.</returns>
    public static IEnumerable<(RandomSourceType Type, IRandomSource Source)> GetAvailableSources()
    {
        var allTypes = Enum.GetValues(typeof(RandomSourceType)).Cast<RandomSourceType>();
        var available = new List<(RandomSourceType, IRandomSource)>();

        foreach (var type in allTypes)
        {
            try
            {
                var source = Create(type);
                available.Add((type, source));
            }
            catch
            {
                // Skip unavailable sources
            }
        }

        return available;
    }
}
