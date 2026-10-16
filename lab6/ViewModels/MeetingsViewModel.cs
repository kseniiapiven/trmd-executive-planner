using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExecutivePlanner.Helpers;
using ExecutivePlanner.Models;
using ExecutivePlanner.Services;
using ExecutivePlanner.Views;
using Microsoft.Extensions.Logging;

namespace ExecutivePlanner.ViewModels;

public partial class MeetingsViewModel : ObservableObject, ILifecycleAware
{
    private const string DraftTitleKey = "draft_title";
    private const string DraftLocationKey = "draft_location";

    private readonly IMeetingRepository _repository;
    private readonly IStateService _stateService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly IAuthService _auth;
    private readonly ILogger<MeetingsViewModel> _logger;
    private bool _stateRestored;

    public MeetingsViewModel(IMeetingRepository repository, IStateService stateService,
        IDialogService dialogService, INavigationService navigationService, IAuthService auth,
        ILogger<MeetingsViewModel> logger)
    {
        _auth = auth;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _logger = logger;
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
    private string userInfo = string.Empty;

    [ObservableProperty]
    private Meeting? selectedMeeting;

    public ObservableCollection<Meeting> Meetings { get; } = new();

    private bool CanAddMeeting() => !string.IsNullOrWhiteSpace(NewTitle);

    [RelayCommand]
    private async Task LoadMeetings()
    {
        OperationResult<List<Meeting>> result;
        try
        {
            result = await _repository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Несподіваний виняток при завантаженні списку зустрічей");
            result = OperationResult<List<Meeting>>.Fail();
        }

        if (!result.Success || result.Value is null)
        {
            _logger.LogWarning("Список зустрічей не оновлено через помилку доступу до даних");
            await _dialogService.ShowErrorAsync(UserMessages.LoadFailed);
            return;
        }

        Meetings.Clear();
        foreach (var item in result.Value) Meetings.Add(item);
        UpdateStatus();
        _logger.LogInformation("Список зустрічей завантажено: {Count}", Meetings.Count);
    }

    [RelayCommand(CanExecute = nameof(CanAddMeeting))]
    private async Task AddMeeting()
    {
#if DEBUG
        // Тестовий збій для кроку 8: фонова задача кидає виняток, який ніхто не очікує.
        if (NewTitle == "#test-unobserved")
        {
            _ = Task.Run(() => { throw new InvalidOperationException("Змодельований неспостережений виняток"); });
            await Task.Delay(300);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            NewTitle = string.Empty;
            return;
        }
#endif
        var meeting = new Meeting
        {
            Title = NewTitle.Trim(),
            Location = NewLocation.Trim(),
            StartTime = NewDate.Date + NewTime
        };

        bool added;
        try
        {
            added = await _repository.InsertAsync(meeting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Несподіваний виняток при додаванні зустрічі");
            added = false;
        }

        if (!added)
        {
            await _dialogService.ShowErrorAsync(UserMessages.SaveFailed); // поля не очищуємо, щоб можна було повторити
            return;
        }

        NewTitle = string.Empty;
        NewLocation = string.Empty;
        await LoadMeetings();
    }

    [RelayCommand]
    private async Task OpenMeeting(Meeting? meeting)
    {
        if (meeting is null) return;
        SelectedMeeting = null; // скидаємо виділення, щоб повторний тап знову спрацьовував
        await _navigationService.GoToAsync($"{nameof(MeetingDetailPage)}?id={meeting.Id}");
    }

    [RelayCommand]
    private async Task Logout()
    {
        _auth.Logout();
        Meetings.Clear();
        UserInfo = string.Empty;
        await _navigationService.GoToAsync("//LoginPage");
    }

    public void OnAppearing()
    {
        _logger.LogDebug("MeetingsViewModel.OnAppearing");
        UserInfo = _auth.CurrentUser is { } user ? $"{user.Login} ({user.Role.ToDisplayName()})" : string.Empty;
        LoadMeetingsCommand.Execute(null);
        RestoreState();
    }

    public void OnDisappearing()
    {
        _logger.LogDebug("MeetingsViewModel.OnDisappearing");
        SaveState();
    }

    // Викликається з App.OnSleep та з OnDisappearing: зберігає незавершену форму введення.
    public void SaveState()
    {
        _stateService.Set(DraftTitleKey, NewTitle);
        _stateService.Set(DraftLocationKey, NewLocation);
        _logger.LogDebug("Чернетку збережено");
    }

    private void RestoreState()
    {
        if (_stateRestored) return;
        _stateRestored = true;
        NewTitle = _stateService.Get(DraftTitleKey);
        NewLocation = _stateService.Get(DraftLocationKey);
        _logger.LogDebug("Чернетку відновлено");
    }

    private void UpdateStatus() =>
        StatusText = Meetings.Count == 0
            ? "Зустрічей поки немає"
            : $"Запланованих зустрічей: {Meetings.Count}";
}
