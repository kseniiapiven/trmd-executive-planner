namespace ExecutivePlanner.Services;

public interface INavigationService
{
    Task GoToAsync(string route);
}
