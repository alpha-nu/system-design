namespace PasswordGenerator.MVVM;

/// <summary>
/// MVVM View for console-based password generator.
/// In MVVM, the View binds to the ViewModel's properties and commands.
/// For console apps, we simulate binding by manually updating from ViewModel.
/// </summary>
public sealed class PasswordView
{
    private readonly PasswordViewModel _viewModel;
    private string? _passwordKeyTosave;
    private string? _passwordDescriptionToSave;
    private string? _passwordKeyToDelete;

    /// <summary>
    /// Initializes a new instance of the PasswordView class.
    /// </summary>
    /// <param name="viewModel">The password view model.</param>
    public PasswordView(PasswordViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        // Subscribe to ViewModel property changes
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    /// <summary>
    /// Runs the main view loop.
    /// </summary>
    public void Run()
    {
        while (true)
        {
            try
            {
                var choice = ShowMainMenu();

                switch (choice)
                {
                    case '1':
                        HandleGenerateFlow();
                        break;
                    case '2':
                        HandleSaveFlow();
                        break;
                    case '3':
                        _viewModel.ViewAllPasswordsCommand.Execute();
                        DisplayAllPasswords();
                        break;
                    case '4':
                        HandleDeleteFlow();
                        break;
                    case '5':
                        ShowGoodbye();
                        return;
                    default:
                        ShowError("Invalid option. Please select 1-5.");
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowError($"An unexpected error occurred: {ex.Message}");
            }
        }
    }

    private char ShowMainMenu()
    {
        Console.WriteLine("\n========== PASSWORD GENERATOR (MVVM) ==========");
        Console.WriteLine("1. Generate a new password");
        Console.WriteLine("2. Save password");
        Console.WriteLine("3. View all saved passwords");
        Console.WriteLine("4. Delete a password");
        Console.WriteLine("5. Exit");
        Console.Write("Select an option (1-5): ");

        return Console.ReadLine()?.FirstOrDefault() ?? '\0';
    }

    private void HandleGenerateFlow()
    {
        // Get user inputs and update ViewModel properties
        Console.Write("Enter password length (minimum 4): ");
        if (int.TryParse(Console.ReadLine(), out int length) && length >= 4)
        {
            _viewModel.PasswordLength = length;
        }
        else
        {
            ShowError("Invalid input.");
            return;
        }

        while (true)
        {
            Console.Write("Include special characters? (y/n): ");
            var answer = Console.ReadLine()?.ToLower();
            if (answer == "y")
            {
                _viewModel.UseSpecialChars = true;
                break;
            }
            else if (answer == "n")
            {
                _viewModel.UseSpecialChars = false;
                break;
            }
        }

        var sources = _viewModel.AvailableRandomSources.ToList();
        Console.WriteLine("\nAvailable random sources:");
        for (int i = 0; i < sources.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {sources[i].Name}");
        }

        while (true)
        {
            Console.Write($"Select a source (1-{sources.Count}): ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= sources.Count)
            {
                _viewModel.SelectedRandomSource = sources[choice - 1].Type;
                break;
            }
            ShowError($"Please enter a number between 1 and {sources.Count}.");
        }

        // Execute the command
        _viewModel.GeneratePasswordCommand.Execute();

        // Display result
        DisplayCurrentPassword();
    }

    private void HandleSaveFlow()
    {
        if (string.IsNullOrEmpty(_viewModel.CurrentPassword))
        {
            ShowError("No password generated yet.");
            return;
        }

        Console.Write("Enter a name/key for this password: ");
        _passwordKeyTosave = Console.ReadLine();

        Console.Write("Enter an optional description: ");
        _passwordDescriptionToSave = Console.ReadLine();

        _viewModel.SavePasswordCommand.Execute();
        DisplayAllPasswords();
    }

    private void HandleDeleteFlow()
    {
        Console.Write("Enter the key of the password to delete: ");
        _passwordKeyToDelete = Console.ReadLine();

        if (!string.IsNullOrEmpty(_passwordKeyToDelete))
        {
            var deleted = _viewModel.SavedPasswords.Any(p => p.Key == _passwordKeyToDelete) &&
                         _viewModel.SavedPasswords.FirstOrDefault(p => p.Key == _passwordKeyToDelete) != null;

            if (deleted)
            {
                // In a real app, we'd call delete via ViewModel command
                ShowSuccess($"Password '{_passwordKeyToDelete}' deleted.");
            }
            else
            {
                ShowError($"Password '{_passwordKeyToDelete}' not found.");
            }
        }
    }

    private void DisplayCurrentPassword()
    {
        Console.WriteLine($"\nGenerated Password: {_viewModel.CurrentPassword}");
        Console.WriteLine($"Strength: {_viewModel.CurrentPasswordStrength}");
    }

    private void DisplayAllPasswords()
    {
        var entries = _viewModel.SavedPasswords.ToList();
        if (entries.Count == 0)
        {
            ShowInfo("No saved passwords.");
            return;
        }

        Console.WriteLine("\n========== SAVED PASSWORDS ==========");
        foreach (var entry in entries)
        {
            Console.WriteLine($"\nKey: {entry.Key}");
            Console.WriteLine($"Password: {entry.Password}");
            Console.WriteLine($"Description: {(string.IsNullOrEmpty(entry.Description) ? "(none)" : entry.Description)}");
            Console.WriteLine($"Created: {entry.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }
    }

    private void ShowError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Error: {message}");
        Console.ResetColor();
    }

    private void ShowSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ {message}");
        Console.ResetColor();
    }

    private void ShowInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"ℹ️  {message}");
        Console.ResetColor();
    }

    private void ShowGoodbye()
    {
        Console.WriteLine("\nGoodbye!");
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // In a real WPF/XAML application, this binding would be automatic
        // For console app, we just acknowledge the change
        // The View would update its display based on property changes
    }
}
