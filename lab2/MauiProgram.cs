using ExecutivePlanner.Services;
using ExecutivePlanner.ViewModels;
using ExecutivePlanner.Views;
using Microsoft.Extensions.Logging;

namespace ExecutivePlanner;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<IMeetingService, MeetingService>();
		builder.Services.AddSingleton<IStateService, PreferencesStateService>();
		builder.Services.AddSingleton<MeetingsViewModel>();
		builder.Services.AddTransient<MeetingDetailViewModel>();
		builder.Services.AddSingleton<MeetingsPage>();
		builder.Services.AddTransient<MeetingDetailPage>();

		return builder.Build();
	}
}
