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
            // Fonts Configuration
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

        // Register core services for Dependency Injection (DI)
        
        // Database Service (Singleton)
        builder.Services.AddSingleton<JournalApp.Data.JournalDatabase>();
        
        // Business Logic Services (Singleton)
        builder.Services.AddSingleton<JournalApp.Services.IJournalService, JournalApp.Services.JournalService>();
        builder.Services.AddSingleton<JournalApp.Services.AuthService>();
        
        // UI Helpers
        builder.Services.AddSingleton<JournalApp.Services.ToastService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Database initialization and seeding is handled automatically by the JournalDatabase service
        // when methods like GetEntriesAsync are called for the first time.

		return app;
	}
}
