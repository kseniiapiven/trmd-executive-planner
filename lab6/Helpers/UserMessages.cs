namespace ExecutivePlanner.Helpers;

// Зрозумілі користувачу повідомлення без технічних деталей винятків.
public static class UserMessages
{
    public const string LoadFailed = "Не вдалося завантажити дані, спробуйте ще раз.";
    public const string SaveFailed = "Не вдалося зберегти дані, спробуйте ще раз.";
    public const string InvalidCredentials = "Невірний логін або пароль.";
    public const string AccessDenied = "Ця дія недоступна для вашої ролі.";
    public const string DeleteFailed = "Не вдалося видалити запис, спробуйте ще раз.";
}
