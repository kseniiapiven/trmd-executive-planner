using ExecutivePlanner.Helpers;
using Microsoft.Extensions.Logging;

namespace ExecutivePlanner.Services;

public class CameraService : ICameraService
{
    private readonly ILogger<CameraService> _logger;

    public CameraService(ILogger<CameraService> logger)
    {
        _logger = logger;
    }

    public async Task<DeviceResult<string>> CapturePhotoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                _logger.LogWarning("Камеру не підтримує цей пристрій");
                return DeviceResult<string>.With(DeviceStatus.NotSupported);
            }

            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (status != PermissionStatus.Granted)
            {
                _logger.LogWarning("Користувач відмовив у дозволі на камеру (статус {Status})", status);
                return DeviceResult<string>.With(DeviceStatus.PermissionDenied);
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null)
            {
                _logger.LogInformation("Фотографування скасовано користувачем");
                return DeviceResult<string>.With(DeviceStatus.Cancelled);
            }

            var directory = Path.Combine(FileSystem.AppDataDirectory, "photos");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, $"{Guid.NewGuid():N}.jpg");

            await using var source = await photo.OpenReadAsync();
            await using var target = File.Create(path);
            await source.CopyToAsync(target);

            _logger.LogInformation("Фото збережено: {Path}", path);
            return DeviceResult<string>.Ok(path);
        }
        catch (PermissionException ex)
        {
            _logger.LogWarning(ex, "Немає дозволу на камеру");
            return DeviceResult<string>.With(DeviceStatus.PermissionDenied);
        }
        catch (FeatureNotSupportedException ex)
        {
            _logger.LogWarning(ex, "Камера не підтримується");
            return DeviceResult<string>.With(DeviceStatus.NotSupported);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка під час фотографування");
            return DeviceResult<string>.With(DeviceStatus.Failed);
        }
    }
}
