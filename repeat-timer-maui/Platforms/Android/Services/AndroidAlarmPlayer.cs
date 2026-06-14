using global::Android.Media;
using global::Android.Provider;
using repeat_timer_maui.Services;

namespace repeat_timer_maui.Platforms.Android.Services;

public sealed class AndroidAlarmPlayer : Java.Lang.Object, IAlarmPlayer
{
    // Holds the active Android MediaPlayer instance while the alarm is playing.
    // The same field is reused so the service can stop and release the player later.
    private MediaPlayer? _mediaPlayer;

    public void PlayLooping()
    {
        // Stop and dispose any previous player before starting a new alarm cycle.
        // This prevents overlapping sounds and avoids leaking MediaPlayer resources.
        Stop();

        // Use the application context because this service is not tied to a page or activity.
        // The global:: prefix ensures the Android framework namespace is used directly.
        var context = global::Android.App.Application.Context;

        // Ask Android for the default alarm sound first.
        // If the device has no alarm sound configured, fall back to the notification sound.
        // As a final fallback, use the system default notification URI.
        var alarmUri = RingtoneManager.GetDefaultUri(RingtoneType.Alarm)
            ?? RingtoneManager.GetDefaultUri(RingtoneType.Notification)
            ?? Settings.System.DefaultNotificationUri;

        // Create a MediaPlayer that is already configured for the selected sound URI.
        // MediaPlayer.Create can return null if the platform cannot open the requested sound.
        _mediaPlayer = MediaPlayer.Create(context, alarmUri);
        if (_mediaPlayer is null)
        {
            // If creation fails, exit safely and leave the service in a stopped state.
            return;
        }

        // Loop forever so the sound continues until the user presses Next or Done.
        _mediaPlayer.Looping = true;

        // Start playback immediately after the player is prepared.
        _mediaPlayer.Start();
    }

    public void Stop()
    {
        // If no player exists, there is nothing to stop or release.
        if (_mediaPlayer is null)
        {
            return;
        }

        // Stop playback only when the player is actively playing.
        // Calling Stop on an idle player can raise state-related errors.
        if (_mediaPlayer.IsPlaying)
        {
            _mediaPlayer.Stop();
        }

        // Release native Android audio resources and dispose the managed wrapper.
        // This is important because MediaPlayer owns platform resources outside the .NET GC.
        _mediaPlayer.Release();
        _mediaPlayer.Dispose();
        _mediaPlayer = null;
    }
}