using ExecutivePlanner.Helpers;
using Microsoft.Extensions.Logging;

namespace ExecutivePlanner.Services;

public class GeolocationService : IGeolocationService
{
    private readonly ILogger<GeolocationService> _logger;

    public GeolocationService(ILogger<GeolocationService> logger)
    {
        _logger = logger;
    }

    public async Task<DeviceResult<GeoPoint>> GetCurrentLocationAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status != PermissionStatus.Granted)
            {
                _logger.LogWarning("Користувач відмовив у дозволі на геолокацію (статус {Status})", status);
                return DeviceResult<GeoPoint>.With(DeviceStatus.PermissionDenied);
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);
            if (location is null)
            {
                _logger.LogWarning("Геолокацію не отримано (результат порожній)");
                return DeviceResult<GeoPoint>.With(DeviceStatus.Failed);
            }

            _logger.LogInformation("Отримано координати: {Latitude}, {Longitude}", location.Latitude, location.Longitude);
            return DeviceResult<GeoPoint>.Ok(new GeoPoint(location.Latitude, location.Longitude));
        }
        catch (FeatureNotEnabledException ex)
        {
            _logger.LogWarning(ex, "Службу геолокації вимкнено");
            return DeviceResult<GeoPoint>.With(DeviceStatus.Disabled);
        }
        catch (FeatureNotSupportedException ex)
        {
            _logger.LogWarning(ex, "Геолокація не підтримується");
            return DeviceResult<GeoPoint>.With(DeviceStatus.NotSupported);
        }
        catch (PermissionException ex)
        {
            _logger.LogWarning(ex, "Немає дозволу на геолокацію");
            return DeviceResult<GeoPoint>.With(DeviceStatus.PermissionDenied);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка отримання геолокації");
            return DeviceResult<GeoPoint>.With(DeviceStatus.Failed);
        }
    }
}
