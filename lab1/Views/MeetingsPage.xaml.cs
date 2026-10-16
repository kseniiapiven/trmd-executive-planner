using ExecutivePlanner.ViewModels;

namespace ExecutivePlanner.Views;

public partial class MeetingsPage : ContentPage
{
    public MeetingsPage(MeetingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
