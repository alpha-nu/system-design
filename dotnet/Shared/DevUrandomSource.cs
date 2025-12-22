namespace PasswordGenerator.Shared;

/// <summary>
/// Unix /dev/urandom random source.
/// Only available on Unix/Linux systems. Reads directly from /dev/urandom device.
/// </summary>
public sealed class DevUrandomSource : IRandomSource
{
    private const string DevUrandomPath = "/dev/urandom";

    /// <inheritdoc/>
    public string Name => "Unix /dev/urandom";

    /// <inheritdoc/>
    public bool IsAvailable => OperatingSystem.IsLinux() || OperatingSystem.IsMacOS();

    /// <inheritdoc/>
    public byte[] GetRandomBytes(int count)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("This random source is only available on Unix/Linux systems.");
        if (count < 0)
            throw new ArgumentException("Count must be non-negative.", nameof(count));

        var bytes = new byte[count];
        try
        {
            using var fs = File.OpenRead(DevUrandomPath);
            int totalRead = 0;
            while (totalRead < count)
            {
                int bytesRead = fs.Read(bytes, totalRead, count - totalRead);
                if (bytesRead == 0) throw new EndOfStreamException();
                totalRead += bytesRead;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to read from {DevUrandomPath}.", ex);
        }

        return bytes;
    }
}
