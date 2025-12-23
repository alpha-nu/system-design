using PasswordGenerator.Shared;

namespace PasswordGenerator.MVC;

/// <summary>
/// Passive Console View contract for MVC.
/// </summary>
public interface IConsoleView
{
    char ShowMainMenu();
    void ShowError(string message);
    void ShowSuccess(string message);
    void ShowInfo(string message);

    int GetPasswordLength();
    bool GetUseSpecialChars();
    int GetRandomSourceChoice(IEnumerable<(RandomSourceType Type, string Name)> sources);

    void ShowGeneratedPassword(string password, PasswordStrength strength);
    string GetPasswordKey();
    string GetPasswordDescription();
    void ShowPasswordList(IEnumerable<PasswordEntry> entries);
    string GetPasswordKeyToDelete();
    void ShowGoodbye();
}
