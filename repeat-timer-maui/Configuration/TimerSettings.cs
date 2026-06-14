namespace repeat_timer_maui.Configuration;

// Root object for appsettings.json.
public sealed class AppSettings
{
    public TimerSettings TimerSettings { get; set; } = new();
}

// Timer-related settings loaded from appsettings.json.
public sealed class TimerSettings
{
    public int DefaultDurationSeconds { get; set; } = 480;
}