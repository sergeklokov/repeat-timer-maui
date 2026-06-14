namespace repeat_timer_maui.Services;

// NullAlarmPlayer is a safe fallback implementation of IAlarmPlayer.
// It is used on platforms where no real alarm playback service is registered.
// The methods intentionally do nothing, which lets the timer logic call
// PlayLooping and Stop without adding platform checks or risking null errors.
public sealed class NullAlarmPlayer : IAlarmPlayer
{
    // Intentionally does nothing because this platform has no alarm sound implementation.
    public void PlayLooping()
    {
    }

    // Intentionally does nothing because this platform has no alarm sound implementation.
    public void Stop()
    {
    }
}