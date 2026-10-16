using SQLite;

namespace ExecutivePlanner.Models;

[Table("Users")]
public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique, NotNull, MaxLength(50)]
    public string Login { get; set; } = string.Empty;

    // Пароль зберігається лише у вигляді солоного хешу (PBKDF2), ніколи у відкритому вигляді.
    [NotNull]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }
}
