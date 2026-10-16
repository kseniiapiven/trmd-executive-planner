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
    private readonly ILogger<MeetingDetailViewModel> _logger;
    private Meeting? _meeting;

    public MeetingDetailViewModel(IMeetingRepository repository, ILogger<MeetingDetailViewModel> logger)
    {
        _logger = logger;
        _repository = repository;
    }

    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string location = string.Empty;
    [ObservableProperty] private string notes = string.Empty;
    [ObservableProperty] private string startDisplay = string.Empty;

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
        _meeting = await _repository.GetByIdAsync(id);
        if (_meeting is null)
        {
            _logger.LogWarning("Зустріч з Id={Id} не знайдено", id);
            return;
        }
        Title = _meeting.Title;
        Location = _meeting.Location;
        Notes = _meeting.Notes;
        StartDisplay = _meeting.StartDisplay;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (_meeting is null || string.IsNullOrWhiteSpace(Title)) return;
        _meeting.Title = Title.Trim();
        _meeting.Location = Location.Trim();
        _meeting.Notes = Notes;
        await _repository.UpdateAsync(_meeting);
        _logger.LogInformation("Зустріч Id={Id} оновлено", _meeting.Id);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (_meeting is null) return;
        await _repository.DeleteAsync(_meeting);
        _logger.LogInformation("Зустріч Id={Id} видалено", _meeting.Id);
        await Shell.Current.GoToAsync("..");
    }

    public void OnAppearing() => _logger.LogDebug("MeetingDetailViewModel.OnAppearing");

    public void OnDisappearing() => _logger.LogDebug("MeetingDetailViewModel.OnDisappearing");
}
