using ExecutivePlanner.Helpers;
using ExecutivePlanner.Models;
using ExecutivePlanner.Services;
using ExecutivePlanner.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ExecutivePlanner.Tests.Unit;

public class MeetingDetailViewModelTests
{
    private readonly Mock<IMeetingRepository> _repository = new();
    private readonly Mock<IDialogService> _dialog = new();
    private readonly Mock<INavigationService> _navigation = new();

    private MeetingDetailViewModel CreateViewModel() =>
        new(_repository.Object, _dialog.Object, _navigation.Object, NullLogger<MeetingDetailViewModel>.Instance);

    private static Meeting SampleMeeting() => new()
    {
        Id = 5,
        Title = "Нарада",
        Location = "Офіс",
        Notes = "Підготувати звіт",
        StartTime = new DateTime(2026, 10, 16, 10, 0, 0)
    };

    private async Task<MeetingDetailViewModel> CreateLoadedViewModelAsync()
    {
        _repository.Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(OperationResult<Meeting?>.Ok(SampleMeeting()));
        var viewModel = CreateViewModel();
        await viewModel.LoadMeetingCommand.ExecuteAsync(5);
        return viewModel;
    }

    [Fact]
    public async Task LoadMeeting_FillsFieldsFromRepository()
    {
        // Arrange + Act
        var viewModel = await CreateLoadedViewModelAsync();

        // Assert
        Assert.Equal("Нарада", viewModel.Title);
        Assert.Equal("Офіс", viewModel.Location);
        Assert.Equal("Підготувати звіт", viewModel.Notes);
        Assert.Equal("16.10.2026 10:00", viewModel.StartDisplay);
    }

    [Fact]
    public void ApplyQueryAttributes_WithMeetingId_LoadsThatMeeting()
    {
        // Arrange
        _repository.Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(OperationResult<Meeting?>.Ok(SampleMeeting()));
        var viewModel = CreateViewModel();

        // Act
        viewModel.ApplyQueryAttributes(new Dictionary<string, object> { ["id"] = "5" });

        // Assert
        _repository.Verify(r => r.GetByIdAsync(5), Times.Once);
        Assert.Equal("Нарада", viewModel.Title);
    }

    [Fact]
    public async Task LoadMeeting_WhenRepositoryFails_ShowsError()
    {
        // Arrange
        _repository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(OperationResult<Meeting?>.Fail());
        var viewModel = CreateViewModel();

        // Act
        await viewModel.LoadMeetingCommand.ExecuteAsync(5);

        // Assert
        _dialog.Verify(d => d.ShowErrorAsync(UserMessages.LoadFailed), Times.Once);
    }

    [Fact]
    public async Task Save_Success_UpdatesMeetingAndNavigatesBack()
    {
        // Arrange
        var viewModel = await CreateLoadedViewModelAsync();
        _repository.Setup(r => r.UpdateAsync(It.IsAny<Meeting>())).ReturnsAsync(true);
        viewModel.Title = "  Нова назва ";

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _repository.Verify(r => r.UpdateAsync(It.Is<Meeting>(m => m.Id == 5 && m.Title == "Нова назва")), Times.Once);
        _navigation.Verify(n => n.GoToAsync(".."), Times.Once);
    }

    [Fact]
    public async Task Save_WhenUpdateFails_ShowsErrorAndStaysOnPage()
    {
        // Arrange
        var viewModel = await CreateLoadedViewModelAsync();
        _repository.Setup(r => r.UpdateAsync(It.IsAny<Meeting>())).ReturnsAsync(false);

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _dialog.Verify(d => d.ShowErrorAsync(UserMessages.SaveFailed), Times.Once);
        _navigation.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Save_WhenRepositoryThrows_DoesNotCrashAndShowsError()
    {
        // Arrange
        var viewModel = await CreateLoadedViewModelAsync();
        _repository.Setup(r => r.UpdateAsync(It.IsAny<Meeting>())).ThrowsAsync(new InvalidOperationException("збій"));

        // Act
        var exception = await Record.ExceptionAsync(() => viewModel.SaveCommand.ExecuteAsync(null));

        // Assert
        Assert.Null(exception);
        _dialog.Verify(d => d.ShowErrorAsync(UserMessages.SaveFailed), Times.Once);
        _navigation.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Save_WithBlankTitle_DoesNotCallRepository(string title)
    {
        // Arrange
        var viewModel = await CreateLoadedViewModelAsync();
        viewModel.Title = title;

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Meeting>()), Times.Never);
        _navigation.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Delete_Success_DeletesMeetingAndNavigatesBack()
    {
        // Arrange
        var viewModel = await CreateLoadedViewModelAsync();
        _repository.Setup(r => r.DeleteAsync(It.IsAny<Meeting>())).ReturnsAsync(true);

        // Act
        await viewModel.DeleteCommand.ExecuteAsync(null);

        // Assert
        _repository.Verify(r => r.DeleteAsync(It.Is<Meeting>(m => m.Id == 5)), Times.Once);
        _navigation.Verify(n => n.GoToAsync(".."), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenRepositoryThrows_DoesNotCrashAndShowsError()
    {
        // Arrange
        var viewModel = await CreateLoadedViewModelAsync();
        _repository.Setup(r => r.DeleteAsync(It.IsAny<Meeting>())).ThrowsAsync(new InvalidOperationException("збій"));

        // Act
        var exception = await Record.ExceptionAsync(() => viewModel.DeleteCommand.ExecuteAsync(null));

        // Assert
        Assert.Null(exception);
        _dialog.Verify(d => d.ShowErrorAsync(UserMessages.DeleteFailed), Times.Once);
        _navigation.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
    }
}
