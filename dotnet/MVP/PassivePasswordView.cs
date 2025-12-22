using PasswordGenerator.Shared;

namespace PasswordGenerator.MVP;

/// <summary>
/// Passive View implementation for MVP pattern.
/// Contains NO business logic - only handles display and input/output.
/// Exposes events for Presenter to subscribe to.
/// </summary>
public sealed class PassivePasswordView : IPasswordView
{
    public event EventHandler? OnGenerateRequested;
    public event EventHandler? OnSaveRequested;
    public event EventHandler? OnViewAllRequested;
    public event EventHandler? OnDeleteRequested;
    public event EventHandler? OnExitRequested;

    private IEnumerable<(RandomSourceType Type, string Name)> _availableRandomSources = Enumerable.Empty<(RandomSourceType, string)>();

    public int PasswordLength { get; set; }
    public bool UseSpecialChars { get; set; }
    public int SelectedRandomSourceIndex { get; set; }
    public string PasswordKey { get; set; } = "";
    public string PasswordDescription { get; set; } = "";
    public string DeletePasswordKey { get; set; } = "";

    public void ShowMainMenu()
    {
        Console.WriteLine("\n========== PASSWORD GENERATOR (MVP) ==========");
        Console.WriteLine("1. Generate a new password");
        Console.WriteLine("2. Save password");
        Console.WriteLine("3. View all saved passwords");
        Console.WriteLine("4. Delete a password");
        Console.WriteLine("5. Exit");
        Console.Write("Select an option (1-5): ");

        var choice = Console.ReadLine()?.FirstOrDefault() ?? '\0';

        switch (choice)
        {
            case '1':
                HandleGenerateFlow();
                break;
            case '2':
                HandleSaveFlow();
                break;
            case '3':
                OnViewAllRequested?.Invoke(this, EventArgs.Empty);
                break;
            case '4':
                HandleDeleteFlow();
                break;
            case '5':
                OnExitRequested?.Invoke(this, EventArgs.Empty);
                break;
            default:
                ShowError("Invalid option. Please select 1-5.");
                break;
        }
    }

    public void ShowError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Error: {message}");
        Console.ResetColor();
    }

    public void ShowSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ {message}");
        Console.ResetColor();
    }

    public void ShowInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"ℹ️  {message}");
        Console.ResetColor();
    }

    public void SetAvailableRandomSources(IEnumerable<(RandomSourceType Type, string Name)> sources)
    {
        _availableRandomSources = sources.ToList();
    }

    public void DisplayGeneratedPassword(string password, PasswordStrength strength)
    {
        Console.WriteLine($"\nGenerated Password: {password}");
        Console.WriteLine($"Strength: {strength}");
    }

    public void DisplayPasswordList(IEnumerable<PasswordEntry> entries)
    {
        var list = entries.ToList();
        if (list.Count == 0)
        {
            ShowInfo("No saved passwords.");
            return;
        }

        Console.WriteLine("\n========== SAVED PASSWORDS ==========");
        foreach (var entry in list)
        {
            Console.WriteLine($"\nKey: {entry.Key}");
            Console.WriteLine($"Password: {entry.Password}");
            Console.WriteLine($"Description: {(string.IsNullOrEmpty(entry.Description) ? "(none)" : entry.Description)}");
            Console.WriteLine($"Created: {entry.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }
    }

    private void HandleGenerateFlow()
    {
        PasswordLength = GetPasswordLength();
        UseSpecialChars = GetUseSpecialChars();
        SelectedRandomSourceIndex = GetRandomSourceChoice();

        OnGenerateRequested?.Invoke(this, EventArgs.Empty);
    }

    private void HandleSaveFlow()
    {
        PasswordKey = GetPasswordKey();
        PasswordDescription = GetPasswordDescription();

        OnSaveRequested?.Invoke(this, EventArgs.Empty);
    }

    private void HandleDeleteFlow()
    {
        DeletePasswordKey = GetPasswordKeyToDelete();
        OnDeleteRequested?.Invoke(this, EventArgs.Empty);
    }

    private int GetPasswordLength()
    {
        while (true)
        {
            Console.Write("Enter password length (minimum 4): ");
            if (int.TryParse(Console.ReadLine(), out int length) && length >= 4)
                return length;
            ShowError("Invalid input. Please enter a number >= 4.");
        }
    }

    private bool GetUseSpecialChars()
    {
        while (true)
        {
            Console.Write("Include special characters? (y/n): ");
            var answer = Console.ReadLine()?.ToLower();
            if (answer == "y") return true;
            if (answer == "n") return false;
            ShowError("Please enter 'y' or 'n'.");
        }
    }

    private int GetRandomSourceChoice()
    {
        var sourceList = _availableRandomSources.ToList();
        if (sourceList.Count == 0)
        {
            ShowError("No random sources available.");
            return -1;
        }

        Console.WriteLine("\nAvailable random sources:");
        for (int i = 0; i < sourceList.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {sourceList[i].Name}");
        }

        while (true)
        {
            Console.Write($"Select a source (1-{sourceList.Count}): ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= sourceList.Count)
                return choice - 1;
            ShowError($"Please enter a number between 1 and {sourceList.Count}.");
        }
    }

    private string GetPasswordKey()
    {
        while (true)
        {
            Console.Write("Enter a name/key for this password: ");
            var key = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(key))
                return key;
            ShowError("Key cannot be empty.");
        }
    }

    private string GetPasswordDescription()
    {
        Console.Write("Enter an optional description (press Enter to skip): ");
        return Console.ReadLine() ?? "";
    }

    private string GetPasswordKeyToDelete()
    {
        while (true)
        {
            Console.Write("Enter the key of the password to delete: ");
            var key = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(key))
                return key;
            ShowError("Key cannot be empty.");
        }
    }
}
