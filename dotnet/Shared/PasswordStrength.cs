namespace PasswordGenerator.Shared;

/// <summary>
/// Enumeration representing the strength level of a generated password.
/// </summary>
public enum PasswordStrength
{
    /// <summary>Weak password: lowercase letters only or fewer than 8 characters.</summary>
    Weak,
    
    /// <summary>Medium password: mixed case and numbers, 8-15 characters.</summary>
    Medium,
    
    /// <summary>Strong password: mixed case, numbers, and special characters, 16+ characters.</summary>
    Strong
}
