using ExecutivePlanner.Helpers;
using ExecutivePlanner.Models;
using ExecutivePlanner.Services;
using ExecutivePlanner.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ExecutivePlanner.Tests.Unit;

public class MeetingsViewModelTests
{
    private readonly Mock<IMeetingRepository> _repository = new();
    private readonly Mock<IStateService> _state = new();
    private readonly Mock<IDialogService> _dialog = new();
    private readonly Mock<INavigationService> _navigation = new();

    private MeetingsViewModel CreateViewModel() =>
        new(_repository.Object, _state.Object, _dialog.Object, _navigation.Object,
            NullLogger<MeetingsViewModel>.Instance);

    private static List<Meeting> TwoMeetings() => new()
    {
        new Meeting { Id = 1, Title = "Нарада", StartTime = new DateTime(2026, 10, 16, 10, 0, 0) },
        new Meeting { Id = 2, Title = "Зустріч з партнером", StartTime = new DateTime(2026, 10, 17, 12, 0, 0) }
    };

    [Fact]
    public async Task LoadMeetings_WhenRepositoryReturnsItems_FillsCollectionAndStatus()
    {
        // Arrange
        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(OperationResult<List<Meeting>>.Ok(TwoMeetings()));
        var viewModel = CreateViewModel();

        // Act
        await viewModel.LoadMeetingsCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(2, viewModel.Meetings.Count);
        Assert.Equal("Запланованих зустрічей: 2", viewModel.StatusText);
        _dialog.Verify(d => d.ShowErrorAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoadMeetings_WhenRepositoryReportsFailure_ShowsErrorAndKeepsCollection()
    {
        // Arrange
        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(OperationResult<List<Meeting>>.Fail());
        var viewModel = CreateViewModel();

        // Act
        await viewModel.LoadMeetingsCommand.ExecuteAsync(null);

        // Assert
        Assert.Empty(viewModel.Meetings);
        _dialog.Verify(d => d.ShowErrorAsync(UserMessages.LoadFailed), Times.Once);
    }

    [Fact]
    public async Task AddMeeting_Success_InsertsMeetingClearsFormAndReloadsList()
    {
        // Arrange
        _repository.Setup(r => r.InsertAsync(It.IsAny<Meeting>())).ReturnsAsync(true);
        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(OperationResult<List<Meeting>>.Ok(TwoMeetings()));
        var viewModel = CreateViewModel();
        viewModel.NewTitle = "  Нарада з інвестором  ";
        viewModel.NewLocation = "Офіс 204";

        // Act
        await viewModel.AddMeetingCommand.ExecuteAsync(null);

        // Assert
        _repository.Verify(r => r.InsertAsync(It.Is<Meeting>(m =>
            m.Title == "Нарада з інвестором" && m.Location == "Офіс 204")), Times.Once);
        Assert.Equal(string.Empty, viewModel.NewTitle);
        Assert.Equal(string.Empty, viewModel.NewLocation);
        Assert.Equal(2, viewModel.Meetings.Count);
    }

    [Fact]
    public async Task AddMeeting_WhenInsertFails_ShowsErrorAndKeepsInputForRetry()
    {
        // Arrange
        _repository.Setup(r => r.InsertAsync(It.IsAny<Meeting>())).ReturnsAsync(false);
        var viewModel = CreateViewModel();
        viewModel.NewTitle = "Нарада";

        // Act
        await viewModel.AddMeetingCommand.ExecuteAsync(null);

        // Assert
        _dialog.Verify(d => d.ShowErrorAsync(UserMessages.SaveFailed), Times.Once);
        Assert.Equal("Нарада", viewModel.NewTitle);
        _repository.Verify(r => r.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task AddMeeting_WhenRepositoryThrows_DoesNotCrashAndShowsError()
    {
        // Arrange
        _repository.Setup(r => r.InsertAsync(It.IsAny<Meeting>()))
            .ThrowsAsync(new InvalidOperationException("збій сховища"));
        var viewModel = CreateViewModel();
        viewModel.NewTitle = "Нарада";

        // Act
        var exception = await Record.ExceptionAsync(() => viewModel.AddMeetingCommand.ExecuteAsync(null));

        // Assert
        Assert.Null(exception);
        _dialog.Verify(d => d.ShowErrorAsync(UserMessages.SaveFailed), Times.Once);
    }

    [Fact]
    public async Task LoadMeetings_WhenRepositoryThrows_DoesNotCrashAndShowsError()
    {
        // Arrange
        _repository.Setup(r => r.GetAllAsync()).ThrowsAsync(new IOException("диск недоступний"));
        var viewModel = CreateViewModel();

        // Act
        var exception = await Record.ExceptionAsync(() => viewModel.LoadMeetingsCommand.ExecuteAsync(null));

        // Assert
        Assert.Null(exception);
        _dialog.Verify(d => d.ShowErrorAsync(UserMessages.LoadFailed), Times.Once);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("\t", false)]
    [InlineData("a", true)]
    [InlineData("Нарада", true)]
    [InlineData("  Нарада  ", true)]
    public void AddMeetingCommand_CanExecute_DependsOnTitle(string title, bool expected)
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.NewTitle = title;

        // Assert
        Assert.Equal(expected, viewModel.AddMeetingCommand.CanExecute(null));
    }

    [Fact]
    public async Task OpenMeeting_NavigatesToDetailPageWithMeetingId()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var meeting = new Meeting { Id = 7, Title = "Нарада" };

        // Act
        await viewModel.OpenMeetingCommand.ExecuteAsync(meeting);

        // Assert
        _navigation.Verify(n => n.GoToAsync("MeetingDetailPage?id=7"), Times.Once);
    }

    [Fact]
    public void SaveState_WritesDraftToStateService()
    {
        // Arrange
        var viewModel = CreateViewModel();
        viewModel.NewTitle = "Чернетка";
        viewModel.NewLocation = "Zoom";

        // Act
        viewModel.SaveState();

        // Assert
        _state.Verify(s => s.Set("draft_title", "Чернетка"), Times.Once);
        _state.Verify(s => s.Set("draft_location", "Zoom"), Times.Once);
    }
}
