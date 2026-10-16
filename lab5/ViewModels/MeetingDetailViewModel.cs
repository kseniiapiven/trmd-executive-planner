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
    private readonly ILogger<MeetingDetailViewModel> _logger;
    private Meeting? _meeting;

    public MeetingDetailViewModel(IMeetingRepository repository, IDialogService dialogService,
        INavigationService navigationService, ILogger<MeetingDetailViewModel> logger)
    {
        _dialogService = dialogService;
        _navigationService = navigationService;
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
    }

    [RelayCommand]
    private async Task Save()
    {
        if (_meeting is null || string.IsNullOrWhiteSpace(Title)) return;
        _meeting.Title = Title.Trim();
        _meeting.Location = Location.Trim();
        _meeting.Notes = Notes;
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
    private async Task Delete()
    {
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
