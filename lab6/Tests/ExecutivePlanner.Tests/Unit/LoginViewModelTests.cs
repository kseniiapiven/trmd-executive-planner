using ExecutivePlanner.Helpers;
using ExecutivePlanner.Services;
using ExecutivePlanner.ViewModels;
using Moq;

namespace ExecutivePlanner.Tests.Unit;

public class LoginViewModelTests
{
    private readonly Mock<IAuthService> _auth = new();
    private readonly Mock<IDialogService> _dialog = new();
    private readonly Mock<INavigationService> _navigation = new();

    private LoginViewModel CreateViewModel() => new(_auth.Object, _dialog.Object, _navigation.Object);

    [Fact]
    public async Task Login_WithValidCredentials_NavigatesToMeetings()
    {
        // Arrange
        _auth.Setup(a => a.LoginAsync("manager", "pass123")).ReturnsAsync(true);
        var viewModel = CreateViewModel();
        viewModel.Login = "manager";
        viewModel.Password = "pass123";

        // Act
        await viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _navigation.Verify(n => n.GoToAsync("//MeetingsPage"), Times.Once);
        Assert.Equal(string.Empty, viewModel.Password);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShowsGenericMessageAndStaysOnPage()
    {
        // Arrange
        _auth.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        var viewModel = CreateViewModel();
        viewModel.Login = "manager";
        viewModel.Password = "wrong";

        // Act
        await viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _dialog.Verify(d => d.ShowErrorAsync(UserMessages.InvalidCredentials), Times.Once);
        _navigation.Verify(n => n.GoToAsync(It.IsAny<string>()), Times.Never);
        Assert.Equal(string.Empty, viewModel.Password);
    }

    [Theory]
    [InlineData("", "", false)]
    [InlineData("manager", "", false)]
    [InlineData("", "pass", false)]
    [InlineData("manager", "pass", true)]
    public void LoginCommand_CanExecute_RequiresLoginAndPassword(string login, string password, bool expected)
    {
        var viewModel = CreateViewModel();
        viewModel.Login = login;
        viewModel.Password = password;

        Assert.Equal(expected, viewModel.LoginCommand.CanExecute(null));
    }
}
