using System.Diagnostics;
using ExecutivePlanner.ViewModels;

namespace ExecutivePlanner;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
		Debug.WriteLine("[Lifecycle] App ctor");
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		Debug.WriteLine("[Lifecycle] App.CreateWindow");
		return base.CreateWindow(activationState);
	}

	protected override void OnStart()
	{
		Debug.WriteLine("[Lifecycle] App.OnStart");
		base.OnStart();
	}

	protected override void OnSleep()
	{
		Debug.WriteLine("[Lifecycle] App.OnSleep");
		IPlatformApplication.Current?.Services.GetService<MeetingsViewModel>()?.SaveState();
		base.OnSleep();
	}

	protected override void OnResume()
	{
		Debug.WriteLine("[Lifecycle] App.OnResume");
		base.OnResume();
	}
}
