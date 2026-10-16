using ExecutivePlanner.Helpers;

namespace ExecutivePlanner.Services;

public interface IGeolocationService
{
    Task<DeviceResult<GeoPoint>> GetCurrentLocationAsync();
}
