using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace JournalApp;

/// <summary>
/// The entry point for the MAUI Application. 
/// Configures the app builder, services, and dependency injection.
/// </summary>
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            //.UseLocalNotification()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

        // Register core services for Dependency Injection (DI)
        // Singleton: Created once and shared across the entire app lifecycle
        builder.Services.AddSingleton<JournalApp.Data.JournalDatabase>();
        builder.Services.AddSingleton<JournalApp.Services.IJournalService, JournalApp.Services.JournalService>();
        builder.Services.AddSingleton<JournalApp.Services.AuthService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // DISABLED: Migration was causing UI thread deadlock
        // The database will initialize and seed data automatically on first use
        // Task.Run(async () =>
        // {
        //     if (await JournalApp.Data.DataMigration.MigrationNeeded(JournalApp.Data.Constants.DatabasePath))
        //     {
        //         await JournalApp.Data.DataMigration.PerformMigration(JournalApp.Data.Constants.DatabasePath);
        //     }
        // }).Wait();

		return app;
	}
}
