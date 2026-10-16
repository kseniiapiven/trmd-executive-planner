using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExecutivePlanner.Helpers;
using ExecutivePlanner.Services;

namespace ExecutivePlanner.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly IDialogService _dialog;
    private readonly INavigationService _navigation;

    public LoginViewModel(IAuthService auth, IDialogService dialog, INavigationService navigation)
    {
        _auth = auth;
        _dialog = dialog;
        _navigation = navigation;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string login = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string password = string.Empty;

    private bool CanLogin() => !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrEmpty(Password);

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        if (!await _auth.LoginAsync(Login, Password))
        {
            Password = string.Empty;
            await _dialog.ShowErrorAsync(UserMessages.InvalidCredentials);
            return;
        }

        Login = string.Empty;
        Password = string.Empty;
        await _navigation.GoToAsync("//MeetingsPage");
    }
}
