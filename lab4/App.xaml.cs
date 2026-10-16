using ExecutivePlanner.ViewModels;
using Microsoft.Extensions.Logging;

namespace ExecutivePlanner;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
		Log("App ctor");
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		Log("App.CreateWindow");
		return base.CreateWindow(activationState);
	}

	protected override void OnStart()
	{
		Log("App.OnStart");
		base.OnStart();
	}

	protected override void OnSleep()
	{
		Log("App.OnSleep");
		IPlatformApplication.Current?.Services.GetService<MeetingsViewModel>()?.SaveState();
		base.OnSleep();
	}

	protected override void OnResume()
	{
		Log("App.OnResume");
		base.OnResume();
	}

	private static void Log(string message) =>
		IPlatformApplication.Current?.Services.GetService<ILogger<App>>()?.LogDebug("[Lifecycle] {Message}", message);
}
