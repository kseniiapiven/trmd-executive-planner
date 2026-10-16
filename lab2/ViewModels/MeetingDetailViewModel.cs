using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using ExecutivePlanner.Helpers;
using ExecutivePlanner.Models;
using ExecutivePlanner.Services;

namespace ExecutivePlanner.ViewModels;

public partial class MeetingDetailViewModel : ObservableObject, IQueryAttributable, ILifecycleAware
{
    private readonly IMeetingService _meetingService;
    private Meeting? _meeting;

    public MeetingDetailViewModel(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string location = string.Empty;
    [ObservableProperty] private string notes = string.Empty;
    [ObservableProperty] private string startDisplay = string.Empty;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var raw) && Guid.TryParse(raw?.ToString(), out var id))
        {
            _meeting = _meetingService.GetById(id);
            if (_meeting is null) return;
            Title = _meeting.Title;
            Location = _meeting.Location;
            Notes = _meeting.Notes;
            StartDisplay = _meeting.StartDisplay;
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (_meeting is null || string.IsNullOrWhiteSpace(Title)) return;
        _meeting.Title = Title.Trim();
        _meeting.Location = Location.Trim();
        _meeting.Notes = Notes;
        _meetingService.Update(_meeting);
        await Shell.Current.GoToAsync("..");
    }

    public void OnAppearing() => Debug.WriteLine($"[Lifecycle] MeetingDetailViewModel.OnAppearing ({Title})");

    public void OnDisappearing() => Debug.WriteLine("[Lifecycle] MeetingDetailViewModel.OnDisappearing");
}
