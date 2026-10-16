namespace ExecutivePlanner.Helpers;

public enum DeviceStatus
{
    Success,
    Cancelled,         // користувач закрив камеру без знімка
    PermissionDenied,  // користувач відмовив у дозволі
    NotSupported,      // на пристрої немає потрібного апаратного модуля
    Disabled,          // службу вимкнено в налаштуваннях (наприклад, GPS)
    Failed             // інша помилка
}

// Результат звернення до апаратного модуля: статус + значення (якщо є).
public sealed record DeviceResult<T>(DeviceStatus Status, T? Value)
{
    public bool IsSuccess => Status == DeviceStatus.Success;
    public static DeviceResult<T> Ok(T value) => new(DeviceStatus.Success, value);
    public static DeviceResult<T> With(DeviceStatus status) => new(status, default);
}

public readonly record struct GeoPoint(double Latitude, double Longitude);
