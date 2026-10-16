namespace ExecutivePlanner.Helpers;

// Зрозумілі користувачу повідомлення без технічних деталей винятків.
public static class AlertHelper
{
    public const string LoadFailed = "Не вдалося завантажити дані, спробуйте ще раз.";
    public const string SaveFailed = "Не вдалося зберегти дані, спробуйте ще раз.";
    public const string DeleteFailed = "Не вдалося видалити запис, спробуйте ще раз.";

    public static async Task ShowErrorAsync(string message)
    {
        var page = Shell.Current;
        if (page is not null)
        {
            await page.DisplayAlert("Помилка", message, "OK");
        }
    }
}
