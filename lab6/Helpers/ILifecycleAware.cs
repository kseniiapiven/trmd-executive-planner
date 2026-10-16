namespace ExecutivePlanner.Helpers;

// Контракт життєвого циклу сторінки на рівні ViewModel:
// code-behind лише делегує виклики, уся логіка живе у ViewModel.
public interface ILifecycleAware
{
    void OnAppearing();
    void OnDisappearing();
}
