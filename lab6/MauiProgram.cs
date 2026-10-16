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
		builder.Services.AddSingleton<IUserRepository>(sp => sp.GetRequiredService<DatabaseService>());
		builder.Services.AddSingleton<IAuthService, AuthService>();
		builder.Services.AddSingleton<ICameraService, CameraService>();
		builder.Services.AddSingleton<IGeolocationService, GeolocationService>();
		builder.Services.AddSingleton<IStateService, PreferencesStateService>();
		builder.Services.AddSingleton<IDialogService, MauiDialogService>();
		builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddSingleton<MeetingsViewModel>();
		builder.Services.AddTransient<MeetingDetailViewModel>();
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddSingleton<MeetingsPage>();
		builder.Services.AddTransient<MeetingDetailPage>();

		var app = builder.Build();
		RegisterGlobalExceptionHandlers(app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalExceptionHandler"));
		return app;
	}

	// Централізований перехоплювач непередбачених винятків: фіксує критичну помилку в лозі.
	private static void RegisterGlobalExceptionHandlers(ILogger logger)
	{
		AppDomain.CurrentDomain.UnhandledException += (_, e) =>
			logger.LogCritical(e.ExceptionObject as Exception,
				"Необроблений виняток (завершення застосунку: {IsTerminating})", e.IsTerminating);

		TaskScheduler.UnobservedTaskException += (_, e) =>
		{
			logger.LogCritical(e.Exception, "Неспостережений виняток у фоновій задачі");
			e.SetObserved();
		};
	}
}
