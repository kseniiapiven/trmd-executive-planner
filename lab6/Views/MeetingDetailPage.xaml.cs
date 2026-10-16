using ExecutivePlanner.Helpers;
using ExecutivePlanner.ViewModels;

namespace ExecutivePlanner.Views;

public partial class MeetingDetailPage : ContentPage
{
    public MeetingDetailPage(MeetingDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as ILifecycleAware)?.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        (BindingContext as ILifecycleAware)?.OnDisappearing();
    }
}
