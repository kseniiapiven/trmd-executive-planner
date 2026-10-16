namespace ExecutivePlanner;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(Views.MeetingDetailPage), typeof(Views.MeetingDetailPage));
	}
}
