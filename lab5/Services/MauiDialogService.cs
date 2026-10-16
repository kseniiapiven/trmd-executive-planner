namespace ExecutivePlanner.Services;

public class MauiDialogService : IDialogService
{
    public async Task ShowErrorAsync(string message)
    {
        var shell = Shell.Current;
        if (shell is not null)
        {
            await shell.DisplayAlert("Помилка", message, "OK");
        }
    }
}
