using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExecutivePlanner.Models;

namespace ExecutivePlanner.ViewModels;

public partial class MeetingsViewModel : ObservableObject
{
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

    public ObservableCollection<Meeting> Meetings { get; } = new();

    private bool CanAddMeeting() => !string.IsNullOrWhiteSpace(NewTitle);

    [RelayCommand(CanExecute = nameof(CanAddMeeting))]
    private void AddMeeting()
    {
        Meetings.Add(new Meeting
        {
            Title = NewTitle.Trim(),
            Location = NewLocation.Trim(),
            StartTime = NewDate.Date + NewTime
        });

        NewTitle = string.Empty;
        NewLocation = string.Empty;
        StatusText = $"Запланованих зустрічей: {Meetings.Count}";
    }
}
