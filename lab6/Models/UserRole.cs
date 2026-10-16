namespace ExecutivePlanner.Models;

public enum UserRole
{
    Manager = 0,   // Керівник
    Assistant = 1  // Помічник
}

public static class UserRoleExtensions
{
    public static string ToDisplayName(this UserRole role) => role switch
    {
        UserRole.Manager => "Керівник",
        UserRole.Assistant => "Помічник",
        _ => role.ToString()
    };
}
