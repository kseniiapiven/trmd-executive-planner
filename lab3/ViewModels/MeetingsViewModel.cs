using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExecutivePlanner.Helpers;
using ExecutivePlanner.Models;
using ExecutivePlanner.Services;
using ExecutivePlanner.Views;

namespace ExecutivePlanner.ViewModels;

public partial class MeetingsViewModel : ObservableObject, ILifecycleAware
{
    private const string DraftTitleKey = "draft_title";
    private const string DraftLocationKey = "draft_location";

    private readonly IMeetingRepository _repository;
    private readonly IStateService _stateService;
    private bool _stateRestored;

    public MeetingsViewModel(IMeetingRepository repository, IStateService stateService)
    {
        _repository = repository;
        _stateService = stateService;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddMeetingCommand))]
    private string newTitle = string.Empty;

    [ObservableProperty]
    private string newLocation = string.Empty;

    [ObservableProperty]
    private DateTime newDate = DateTime.Today;

    [ObservableProperty]
    private TimeSpan newTime = new(10, 0, 0);

    [ObservableProperty]
    private string statusText = "Зустрічей поки немає";

    [ObservableProperty]
    private Meeting? selectedMeeting;

    public ObservableCollection<Meeting> Meetings { get; } = new();

    private bool CanAddMeeting() => !string.IsNullOrWhiteSpace(NewTitle);

    [RelayCommand]
    private async Task LoadMeetings()
    {
        var items = await _repository.GetAllAsync();
        Meetings.Clear();
        foreach (var item in items) Meetings.Add(item);
        UpdateStatus();
    }

    [RelayCommand(CanExecute = nameof(CanAddMeeting))]
    private async Task AddMeeting()
    {
        await _repository.InsertAsync(new Meeting
        {
            Title = NewTitle.Trim(),
            Location = NewLocation.Trim(),
            StartTime = NewDate.Date + NewTime
        });

        NewTitle = string.Empty;
        NewLocation = string.Empty;
        await LoadMeetings();
    }

    [RelayCommand]
    private async Task OpenMeeting(Meeting? meeting)
    {
        if (meeting is null) return;
        SelectedMeeting = null; // скидаємо виділення, щоб повторний тап знову спрацьовував
        await Shell.Current.GoToAsync($"{nameof(MeetingDetailPage)}?id={meeting.Id}");
    }

    public void OnAppearing()
    {
        Debug.WriteLine("[Lifecycle] MeetingsViewModel.OnAppearing");
        LoadMeetingsCommand.Execute(null);
        RestoreState();
    }

    public void OnDisappearing()
    {
        Debug.WriteLine("[Lifecycle] MeetingsViewModel.OnDisappearing");
        SaveState();
    }

    // Викликається з App.OnSleep та з OnDisappearing: зберігає незавершену форму введення.
    public void SaveState()
    {
        _stateService.Set(DraftTitleKey, NewTitle);
        _stateService.Set(DraftLocationKey, NewLocation);
        Debug.WriteLine($"[State] saved draft: '{NewTitle}' / '{NewLocation}'");
    }

    private void RestoreState()
    {
        if (_stateRestored) return;
        _stateRestored = true;
        NewTitle = _stateService.Get(DraftTitleKey);
        NewLocation = _stateService.Get(DraftLocationKey);
        Debug.WriteLine($"[State] restored draft: '{NewTitle}' / '{NewLocation}'");
    }

    private void UpdateStatus() =>
        StatusText = Meetings.Count == 0
            ? "Зустрічей поки немає"
            : $"Запланованих зустрічей: {Meetings.Count}";
}
