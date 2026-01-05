using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace JournalApp;

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
        builder.Services.AddSingleton<JournalApp.Data.JournalDatabase>();
        builder.Services.AddSingleton<JournalApp.Services.IJournalService, JournalApp.Services.JournalService>();
        builder.Services.AddSingleton<JournalApp.Services.AuthService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
