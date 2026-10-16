using ExecutivePlanner.Helpers;

namespace ExecutivePlanner.Services;

// Фотографування: перевіряє/запитує дозвіл, знімає фото й зберігає його у внутрішньому сховищі застосунку.
public interface ICameraService
{
    // У разі успіху Value - шлях до збереженого файлу.
    Task<DeviceResult<string>> CapturePhotoAsync();
}
