namespace ExecutivePlanner.Services;

// Показ повідомлень користувачу; інтерфейс дозволяє підміняти UI у модульних тестах.
public interface IDialogService
{
    Task ShowErrorAsync(string message);
}
