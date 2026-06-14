using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using repeat_timer_maui.Configuration;
using repeat_timer_maui.Services;

#if ANDROID
using repeat_timer_maui.Platforms.Android.Services;
#endif

namespace repeat_timer_maui;

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

        // Load app settings from the packaged MAUI asset so it works on Android.
        using var appSettingsStream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
        builder.Configuration.AddJsonStream(appSettingsStream);

        var appSettings = new AppSettings();
        builder.Configuration.Bind(appSettings);
        builder.Services.AddSingleton(appSettings);
        builder.Services.AddSingleton(appSettings.TimerSettings);

        // Register the Android alarm player on Android and a no-op fallback elsewhere.
#if ANDROID
        builder.Services.AddSingleton<IAlarmPlayer, AndroidAlarmPlayer>();
#else
        builder.Services.AddSingleton<IAlarmPlayer, NullAlarmPlayer>();
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
