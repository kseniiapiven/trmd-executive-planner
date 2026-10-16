namespace ExecutivePlanner.Helpers;

// Контрольований результат операції: ViewModel бачить лише ознаку успіху, технічні деталі лишаються в лозі.
public sealed record OperationResult<T>(bool Success, T? Value)
{
    public static OperationResult<T> Ok(T value) => new(true, value);
    public static OperationResult<T> Fail() => new(false, default);
}
