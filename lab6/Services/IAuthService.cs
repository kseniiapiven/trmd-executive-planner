using ExecutivePlanner.Models;

namespace ExecutivePlanner.Services;

// Спрощена локальна автентифікація та перевірка ролі поточного користувача.
public interface IAuthService
{
    User? CurrentUser { get; }
    bool IsAuthenticated { get; }
    Task<bool> LoginAsync(string login, string password);
    void Logout();
    bool IsInRole(UserRole role);
}
