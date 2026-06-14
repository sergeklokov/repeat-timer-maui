namespace repeat_timer_maui.Services;

public interface IAlarmPlayer
{
    // Start alarm playback in a continuous loop.
    void PlayLooping();

    // Stop any active alarm playback.
    void Stop();
}