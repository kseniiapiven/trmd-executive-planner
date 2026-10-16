using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExecutivePlanner.Helpers;
using ExecutivePlanner.Models;
using ExecutivePlanner.Services;
using Microsoft.Extensions.Logging;

namespace ExecutivePlanner.ViewModels;

public partial class MeetingDetailViewModel : ObservableObject, IQueryAttributable, ILifecycleAware
{
    private readonly IMeetingRepository _repository;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly IAuthService _auth;
    private readonly ICameraService _camera;
    private readonly IGeolocationService _geolocation;
    private readonly ILogger<MeetingDetailViewModel> _logger;
    private Meeting? _meeting;

    public MeetingDetailViewModel(IMeetingRepository repository, IDialogService dialogService,
        INavigationService navigationService, IAuthService auth, ICameraService camera,
        IGeolocationService geolocation, ILogger<MeetingDetailViewModel> logger)
    {
        _auth = auth;
        _camera = camera;
        _geolocation = geolocation;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _logger = logger;
        _repository = repository;
    }

    // Видалення дозволене лише Керівнику: кнопка прихована для інших ролей, а команда додатково перевіряє роль.
    public bool CanDelete => _auth.IsInRole(UserRole.Manager);

    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string location = string.Empty;
    [ObservableProperty] private string notes = string.Empty;
    [ObservableProperty] private string startDisplay = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasImage))]
    private string imagePath = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CoordinatesText))]
    private double? latitude;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CoordinatesText))]
    private double? longitude;

    public bool HasImage => !string.IsNullOrEmpty(ImagePath);

    public string CoordinatesText => Latitude is { } lat && Longitude is { } lon
        ? string.Create(CultureInfo.InvariantCulture, $"Координати: {lat:F5}, {lon:F5}")
        : "Координати не визначено";

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var raw) && int.TryParse(raw?.ToString(), out var id))
        {
            LoadMeetingCommand.Execute(id);
        }
    }

    [RelayCommand]
    private async Task LoadMeeting(int id)
    {
        OperationResult<Meeting?> result;
        try
        {
            result = await _repository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Несподіваний виняток при читанні зустрічі Id={Id}", id);
            result = OperationResult<Meeting?>.Fail();
        }

        if (!result.Success)
        {
            await _dialogService.ShowErrorAsync(UserMessages.LoadFailed);
            return;
        }

        _meeting = result.Value;
        if (_meeting is null) return;
        Title = _meeting.Title;
        Location = _meeting.Location;
        Notes = _meeting.Notes;
        StartDisplay = _meeting.StartDisplay;
        ImagePath = _meeting.ImagePath;
        Latitude = _meeting.Latitude;
        Longitude = _meeting.Longitude;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (_meeting is null || string.IsNullOrWhiteSpace(Title)) return;
        _meeting.Title = Title.Trim();
        _meeting.Location = Location.Trim();
        _meeting.Notes = Notes;
        _meeting.ImagePath = ImagePath;
        _meeting.Latitude = Latitude;
        _meeting.Longitude = Longitude;
        bool updated;
        try
        {
            updated = await _repository.UpdateAsync(_meeting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Несподіваний виняток при оновленні зустрічі Id={Id}", _meeting.Id);
            updated = false;
        }

        if (!updated)
        {
            await _dialogService.ShowErrorAsync(UserMessages.SaveFailed);
            return;
        }

        await _navigationService.GoToAsync("..");
    }

    [RelayCommand]
    private async Task TakePhoto()
    {
        var result = await _camera.CapturePhotoAsync();
        switch (result.Status)
        {
            case DeviceStatus.Success:
                ImagePath = result.Value ?? string.Empty;
                break;
            case DeviceStatus.Cancelled:
                break;
            case DeviceStatus.PermissionDenied:
                await _dialogService.ShowErrorAsync(UserMessages.CameraPermissionDenied);
                break;
            case DeviceStatus.NotSupported:
                await _dialogService.ShowErrorAsync(UserMessages.CameraUnavailable);
                break;
            default:
                await _dialogService.ShowErrorAsync(UserMessages.PhotoFailed);
                break;
        }
    }

    [RelayCommand]
    private async Task DetectLocation()
    {
        var result = await _geolocation.GetCurrentLocationAsync();
        switch (result.Status)
        {
            case DeviceStatus.Success:
                Latitude = result.Value.Latitude;
                Longitude = result.Value.Longitude;
                break;
            case DeviceStatus.PermissionDenied:
                await _dialogService.ShowErrorAsync(UserMessages.LocationPermissionDenied);
                break;
            case DeviceStatus.Disabled:
                await _dialogService.ShowErrorAsync(UserMessages.LocationDisabled);
                break;
            case DeviceStatus.NotSupported:
                await _dialogService.ShowErrorAsync(UserMessages.LocationUnavailable);
                break;
            default:
                await _dialogService.ShowErrorAsync(UserMessages.LocationFailed);
                break;
        }
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (!_auth.IsInRole(UserRole.Manager))
        {
            _logger.LogWarning("Спробу видалення зустрічі відхилено: недостатньо прав (користувач {Login})", _auth.CurrentUser?.Login);
            await _dialogService.ShowErrorAsync(UserMessages.AccessDenied);
            return;
        }

        if (_meeting is null) return;
        bool deleted;
        try
        {
            deleted = await _repository.DeleteAsync(_meeting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Несподіваний виняток при видаленні зустрічі Id={Id}", _meeting.Id);
            deleted = false;
        }

        if (!deleted)
        {
            await _dialogService.ShowErrorAsync(UserMessages.DeleteFailed);
            return;
        }

        await _navigationService.GoToAsync("..");
    }

    public void OnAppearing() => _logger.LogDebug("MeetingDetailViewModel.OnAppearing");

    public void OnDisappearing() => _logger.LogDebug("MeetingDetailViewModel.OnDisappearing");
}
