namespace PasswordGenerator.MVC;

/// <summary>
/// Passive View for MVC pattern.
/// Contains NO business logic - only handles display and basic input.
/// </summary>
public sealed class ConsoleView
{
    /// <summary>
    /// Displays the main menu and returns the user's choice.
    /// </summary>
    public char ShowMainMenu()
    {
        Console.WriteLine("\n========== PASSWORD GENERATOR (MVC) ==========");
        Console.WriteLine("1. Generate a new password");
        Console.WriteLine("2. Save password");
        Console.WriteLine("3. View all saved passwords");
        Console.WriteLine("4. Delete a password");
        Console.WriteLine("5. Exit");
        Console.Write("Select an option (1-5): ");

        return Console.ReadLine()?.FirstOrDefault() ?? '\0';
    }

    /// <summary>
    /// Displays an error message.
    /// </summary>
    public void ShowError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Error: {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Displays a success message.
    /// </summary>
    public void ShowSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Displays an information message.
    /// </summary>
    public void ShowInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"ℹ️  {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Gets the desired password length from the user.
    /// </summary>
    public int GetPasswordLength()
    {
        while (true)
        {
            Console.Write("Enter password length (minimum 4): ");
            if (int.TryParse(Console.ReadLine(), out int length) && length >= 4)
                return length;
            ShowError("Invalid input. Please enter a number >= 4.");
        }
    }

    /// <summary>
    /// Asks the user if they want to include special characters.
    /// </summary>
    public bool GetUseSpecialChars()
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

    /// <summary>
    /// Gets the random source selection from the user.
    /// </summary>
    public int GetRandomSourceChoice(IEnumerable<(RandomSourceType Type, string Name)> sources)
    {
        var sourceList = sources.ToList();
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

    /// <summary>
    /// Displays the generated password and its strength.
    /// </summary>
    public void ShowGeneratedPassword(string password, PasswordStrength strength)
    {
        Console.WriteLine($"\nGenerated Password: {password}");
        Console.WriteLine($"Strength: {strength}");
    }

    /// <summary>
    /// Gets the name/key for saving a password.
    /// </summary>
    public string GetPasswordKey()
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

    /// <summary>
    /// Gets the optional description for a password.
    /// </summary>
    public string GetPasswordDescription()
    {
        Console.Write("Enter an optional description (press Enter to skip): ");
        return Console.ReadLine() ?? "";
    }

    /// <summary>
    /// Displays all saved passwords.
    /// </summary>
    public void ShowPasswordList(IEnumerable<PasswordEntry> entries)
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

    /// <summary>
    /// Gets the key of a password to delete.
    /// </summary>
    public string GetPasswordKeyToDelete()
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

    /// <summary>
    /// Displays a goodbye message.
    /// </summary>
    public void ShowGoodbye()
    {
        Console.WriteLine("\nGoodbye!");
    }
}
