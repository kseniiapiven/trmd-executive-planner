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
    private readonly Mock<IAuthService> _auth = new();
    private readonly Mock<ICameraService> _camera = new();
    private readonly Mock<IGeolocationService> _geolocation = new();

    public MeetingDetailViewModelTests()
    {
        // За замовчуванням поточний користувач - Керівник
        _auth.Setup(a => a.IsInRole(UserRole.Manager)).Returns(true);
    }

    private MeetingDetailViewModel CreateViewModel() =>
        new(_repository.Object, _dialog.Object, _navigation.Object, _auth.Object, _camera.Object,
            _geolocation.Object, NullLogger<MeetingDetailViewModel>.Instance);

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

    [Fact]
    public async Task Delete_AsAssistant_IsDeniedAndDoesNotTouchRepository()
    {
        // Arrange: Помічник не має права видаляти
        _auth.Setup(a => a.IsInRole(UserRole.Manager)).Returns(false);
        var viewModel = await CreateLoadedViewModelAsync();

        // Act
        await viewModel.DeleteCommand.ExecuteAsync(null);

        // Assert: роль перевіряється в самій команді, а не лише приховуванням кнопки
        _repository.Verify(r => r.DeleteAsync(It.IsAny<Meeting>()), Times.Never);
        _dialog.Verify(d => d.ShowErrorAsync(UserMessages.AccessDenied), Times.Once);
        _navigation.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CanDelete_ReflectsManagerRole(bool isManager)
    {
        _auth.Setup(a => a.IsInRole(UserRole.Manager)).Returns(isManager);

        Assert.Equal(isManager, CreateViewModel().CanDelete);
    }

    [Fact]
    public async Task TakePhoto_Success_SetsImagePathAndShowsImage()
    {
        // Arrange
        _camera.Setup(c => c.CapturePhotoAsync()).ReturnsAsync(DeviceResult<string>.Ok("/data/photos/abc.jpg"));
        var viewModel = CreateViewModel();

        // Act
        await viewModel.TakePhotoCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("/data/photos/abc.jpg", viewModel.ImagePath);
        Assert.True(viewModel.HasImage);
        _dialog.Verify(d => d.ShowErrorAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task TakePhoto_WhenCancelled_ChangesNothingAndShowsNoError()
    {
        _camera.Setup(c => c.CapturePhotoAsync()).ReturnsAsync(DeviceResult<string>.With(DeviceStatus.Cancelled));
        var viewModel = CreateViewModel();

        await viewModel.TakePhotoCommand.ExecuteAsync(null);

        Assert.False(viewModel.HasImage);
        _dialog.Verify(d => d.ShowErrorAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(DeviceStatus.PermissionDenied, UserMessages.CameraPermissionDenied)]
    [InlineData(DeviceStatus.NotSupported, UserMessages.CameraUnavailable)]
    [InlineData(DeviceStatus.Failed, UserMessages.PhotoFailed)]
    public async Task TakePhoto_OnProblem_ShowsUnderstandableMessageAndKeepsAppRunning(DeviceStatus status, string expectedMessage)
    {
        // Arrange
        _camera.Setup(c => c.CapturePhotoAsync()).ReturnsAsync(DeviceResult<string>.With(status));
        var viewModel = CreateViewModel();

        // Act
        var exception = await Record.ExceptionAsync(() => viewModel.TakePhotoCommand.ExecuteAsync(null));

        // Assert
        Assert.Null(exception);
        Assert.False(viewModel.HasImage);
        _dialog.Verify(d => d.ShowErrorAsync(expectedMessage), Times.Once);
    }

    [Fact]
    public async Task DetectLocation_Success_SetsCoordinatesAndFormatsText()
    {
        // Arrange
        _geolocation.Setup(g => g.GetCurrentLocationAsync())
            .ReturnsAsync(DeviceResult<GeoPoint>.Ok(new GeoPoint(50.4501, 30.5234)));
        var viewModel = CreateViewModel();

        // Act
        await viewModel.DetectLocationCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(50.4501, viewModel.Latitude);
        Assert.Equal(30.5234, viewModel.Longitude);
        Assert.Equal("Координати: 50.45010, 30.52340", viewModel.CoordinatesText);
    }

    [Fact]
    public void CoordinatesText_WithoutLocation_SaysNotDetermined()
    {
        Assert.Equal("Координати не визначено", CreateViewModel().CoordinatesText);
    }

    [Theory]
    [InlineData(DeviceStatus.PermissionDenied, UserMessages.LocationPermissionDenied)]
    [InlineData(DeviceStatus.Disabled, UserMessages.LocationDisabled)]
    [InlineData(DeviceStatus.NotSupported, UserMessages.LocationUnavailable)]
    [InlineData(DeviceStatus.Failed, UserMessages.LocationFailed)]
    public async Task DetectLocation_OnProblem_ShowsUnderstandableMessageAndKeepsAppRunning(DeviceStatus status, string expectedMessage)
    {
        // Arrange
        _geolocation.Setup(g => g.GetCurrentLocationAsync()).ReturnsAsync(DeviceResult<GeoPoint>.With(status));
        var viewModel = CreateViewModel();

        // Act
        var exception = await Record.ExceptionAsync(() => viewModel.DetectLocationCommand.ExecuteAsync(null));

        // Assert
        Assert.Null(exception);
        Assert.Null(viewModel.Latitude);
        _dialog.Verify(d => d.ShowErrorAsync(expectedMessage), Times.Once);
    }

    [Fact]
    public async Task Save_PersistsPhotoPathAndCoordinatesOnMeeting()
    {
        // Arrange
        var viewModel = await CreateLoadedViewModelAsync();
        _camera.Setup(c => c.CapturePhotoAsync()).ReturnsAsync(DeviceResult<string>.Ok("/data/photos/abc.jpg"));
        _geolocation.Setup(g => g.GetCurrentLocationAsync())
            .ReturnsAsync(DeviceResult<GeoPoint>.Ok(new GeoPoint(50.45, 30.52)));
        _repository.Setup(r => r.UpdateAsync(It.IsAny<Meeting>())).ReturnsAsync(true);
        await viewModel.TakePhotoCommand.ExecuteAsync(null);
        await viewModel.DetectLocationCommand.ExecuteAsync(null);

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _repository.Verify(r => r.UpdateAsync(It.Is<Meeting>(m =>
            m.ImagePath == "/data/photos/abc.jpg" && m.Latitude == 50.45 && m.Longitude == 30.52)), Times.Once);
    }
}
