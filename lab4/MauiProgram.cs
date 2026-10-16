using ExecutivePlanner.Helpers;
using ExecutivePlanner.Services;
using ExecutivePlanner.ViewModels;
using ExecutivePlanner.Views;
using Microsoft.Extensions.Logging;

namespace ExecutivePlanner;

public static class MauiProgram
{
	public static string LogFilePath => Path.Combine(FileSystem.AppDataDirectory, "executiveplanner.log");

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

		builder.Logging.SetMinimumLevel(LogLevel.Debug);
#if DEBUG
		builder.Logging.AddDebug();
#endif
		builder.Logging.AddProvider(new FileLoggerProvider(LogFilePath));

		builder.Services.AddSingleton<DatabaseService>();
		builder.Services.AddSingleton<IMeetingRepository>(sp => sp.GetRequiredService<DatabaseService>());
		builder.Services.AddSingleton<IStateService, PreferencesStateService>();
		builder.Services.AddSingleton<MeetingsViewModel>();
		builder.Services.AddTransient<MeetingDetailViewModel>();
		builder.Services.AddSingleton<MeetingsPage>();
		builder.Services.AddTransient<MeetingDetailPage>();

		return builder.Build();
	}
}
