namespace ExecutivePlanner.Helpers;

// Зрозумілі користувачу повідомлення без технічних деталей винятків.
public static class UserMessages
{
    public const string LoadFailed = "Не вдалося завантажити дані, спробуйте ще раз.";
    public const string SaveFailed = "Не вдалося зберегти дані, спробуйте ще раз.";
    public const string InvalidCredentials = "Невірний логін або пароль.";
    public const string AccessDenied = "Ця дія недоступна для вашої ролі.";
    public const string CameraPermissionDenied = "Немає дозволу на використання камери. Дозвольте доступ у налаштуваннях пристрою.";
    public const string CameraUnavailable = "Камера недоступна на цьому пристрої.";
    public const string PhotoFailed = "Не вдалося зробити фото, спробуйте ще раз.";
    public const string LocationPermissionDenied = "Немає дозволу на визначення місцезнаходження. Дозвольте доступ у налаштуваннях пристрою.";
    public const string LocationDisabled = "Службу геолокації вимкнено. Увімкніть її в налаштуваннях пристрою.";
    public const string LocationUnavailable = "Геолокація недоступна на цьому пристрої.";
    public const string LocationFailed = "Не вдалося визначити місцезнаходження, спробуйте ще раз.";
    public const string DeleteFailed = "Не вдалося видалити запис, спробуйте ще раз.";
}
