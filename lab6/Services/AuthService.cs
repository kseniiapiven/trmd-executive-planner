using ExecutivePlanner.Helpers;
using ExecutivePlanner.Models;
using Microsoft.Extensions.Logging;

namespace ExecutivePlanner.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository users, ILogger<AuthService> logger)
    {
        _users = users;
        _logger = logger;
    }

    public User? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser is not null;

    public async Task<bool> LoginAsync(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrEmpty(password)) return false;

        var result = await _users.GetByLoginAsync(login.Trim());
        var user = result.Success ? result.Value : null;

        // Однакова відповідь для «немає користувача» і «хибний пароль», щоб не підказувати, який логін існує.
        if (user is null || !PasswordHasher.Verify(password, user.PasswordHash))
        {
            _logger.LogWarning("Невдала спроба входу для логіна {Login}", login.Trim());
            return false;
        }

        CurrentUser = user;
        _logger.LogInformation("Вхід виконано: {Login}, роль {Role}", user.Login, user.Role);
        return true;
    }

    public void Logout()
    {
        if (CurrentUser is not null) _logger.LogInformation("Вихід користувача {Login}", CurrentUser.Login);
        CurrentUser = null;
    }

    public bool IsInRole(UserRole role) => CurrentUser?.Role == role;
}
