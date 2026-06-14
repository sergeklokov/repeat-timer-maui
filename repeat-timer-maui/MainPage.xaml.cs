namespace repeat_timer_maui;

using System.Text;
using repeat_timer_maui.Services;

public partial class MainPage : ContentPage
{
    // Alarm playback service resolved from dependency injection.
    private readonly IAlarmPlayer _alarmPlayer;

    // Active UI timer used for the one-second countdown tick.
    private IDispatcherTimer? _countdownTimer;

    // Initial duration captured when the user starts a timer.
    private int _initialDurationSeconds = 4;

    // Current duration after Increase/Decrease adjustments.
    private int _currentDurationSeconds = 4;

    // Remaining seconds in the active countdown.
    private int _remainingSeconds = 4;

    // Tracks whether the alarm is currently looping.
    private bool _alarmPlaying;

    public MainPage()
    {
        InitializeComponent();

        // Resolve the platform alarm service. Fall back to a no-op implementation when unavailable.
        _alarmPlayer = Application.Current?.Handler?.MauiContext?.Services.GetService(typeof(IAlarmPlayer)) as IAlarmPlayer
            ?? new NullAlarmPlayer();

        // Initialize the page with the default four-second duration.
        SetDurationSeconds(4, updateInitialDuration: true);
        UpdateStatusLabel();
    }

    private void OnDurationEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        // Strip unsupported characters so the entry only keeps digits and one colon.
        var sanitizedText = SanitizeDurationInput(e.NewTextValue);
        if (sanitizedText != e.NewTextValue)
        {
            SetDurationText(sanitizedText);
            return;
        }

        // When the typed value is valid, keep internal duration values in sync with the entry.
        if (TryParseDuration(sanitizedText, out var totalSeconds))
        {
            _initialDurationSeconds = totalSeconds;
            _currentDurationSeconds = totalSeconds;
            _remainingSeconds = totalSeconds;
        }

        UpdateDurationState(sanitizedText);
        UpdateStatusLabel();
    }

    private void OnStartButtonClicked(object? sender, EventArgs e)
    {
        // Start uses the entered value as the new baseline for 10% adjustments.
        if (!TryGetEnteredDuration(out var durationSeconds))
        {
            StatusLabel.Text = "Enter a valid duration.";
            StatusLabel.TextColor = Colors.Red;
            return;
        }

        _alarmPlayer.Stop();
        _alarmPlaying = false;
        _initialDurationSeconds = durationSeconds;
        _currentDurationSeconds = durationSeconds;
        StartCountdown(durationSeconds);
    }

    private void OnNextButtonClicked(object? sender, EventArgs e)
    {
        // Next stops any alarm and restarts the timer using the current adjusted duration.
        _alarmPlayer.Stop();
        _alarmPlaying = false;
        StartCountdown(_currentDurationSeconds);
    }

    private void OnIncreaseButtonClicked(object? sender, EventArgs e)
    {
        // Increase adds 10% of the initial started duration, with a minimum step of one second.
        var delta = Math.Max(1, (int)Math.Round(_initialDurationSeconds * 0.1, MidpointRounding.AwayFromZero));
        SetDurationSeconds(_currentDurationSeconds + delta, updateInitialDuration: false);
    }

    private void OnDecreaseButtonClicked(object? sender, EventArgs e)
    {
        // Decrease removes 10% of the initial started duration, but never goes below one second.
        var delta = Math.Max(1, (int)Math.Round(_initialDurationSeconds * 0.1, MidpointRounding.AwayFromZero));
        var newDuration = Math.Max(1, _currentDurationSeconds - delta);
        SetDurationSeconds(newDuration, updateInitialDuration: false);
    }

    private void OnDoneButtonClicked(object? sender, EventArgs e)
    {
        // Done cancels countdown activity and silences any looping alarm.
        StopCountdown();
        _alarmPlayer.Stop();
        _alarmPlaying = false;
        _remainingSeconds = _currentDurationSeconds;
        UpdateStatusLabel("Stopped");
    }

    private void StartCountdown(int durationSeconds)
    {
        // Replace any previous timer and start a fresh one-second countdown loop.
        StopCountdown();

        _remainingSeconds = Math.Max(1, durationSeconds);
        _countdownTimer = Dispatcher.CreateTimer();
        _countdownTimer.Interval = TimeSpan.FromSeconds(1);
        _countdownTimer.Tick += OnCountdownTick;
        _countdownTimer.Start();

        UpdateStatusLabel();
    }

    private void StopCountdown()
    {
        // Dispose the active UI timer safely.
        if (_countdownTimer is null)
        {
            return;
        }

        _countdownTimer.Stop();
        _countdownTimer.Tick -= OnCountdownTick;
        _countdownTimer = null;
    }

    private void OnCountdownTick(object? sender, EventArgs e)
    {
        // Count down once per second. When zero is reached, stop the timer and loop the alarm.
        _remainingSeconds--;

        if (_remainingSeconds <= 0)
        {
            StopCountdown();
            _remainingSeconds = 0;
            _alarmPlaying = true;
            _alarmPlayer.PlayLooping();
            UpdateStatusLabel("Time is up");
            return;
        }

        UpdateStatusLabel();
    }

    private void SetDurationText(string? text)
    {
        // Avoid recursive text updates when sanitizing user input.
        if (DurationEntry.Text != text)
        {
            DurationEntry.Text = text;
            return;
        }

        UpdateDurationState(text);
    }

    private void SetDurationSeconds(int seconds, bool updateInitialDuration)
    {
        // Apply a new duration to the entry and internal timer state.
        var safeSeconds = Math.Max(1, seconds);
        if (updateInitialDuration)
        {
            _initialDurationSeconds = safeSeconds;
        }

        _currentDurationSeconds = safeSeconds;
        _remainingSeconds = safeSeconds;
        SetDurationText(safeSeconds.ToString());
        UpdateStatusLabel();
    }

    private void UpdateDurationState(string? text)
    {
        // Show the parsed duration in both total seconds and m:ss format.
        if (TryParseDuration(text, out var totalSeconds))
        {
            DurationHintLabel.Text = $"Parsed: {totalSeconds} second(s) ({FormatDuration(totalSeconds)})";
            DurationHintLabel.TextColor = Colors.Green;
            return;
        }

        DurationHintLabel.Text = "Enter seconds or mm:ss";
        DurationHintLabel.TextColor = Colors.Gray;
    }

    private void UpdateStatusLabel(string? prefix = null)
    {
        // Centralize all status text updates for ready, running, stopped, and alarm states.
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            StatusLabel.Text = $"{prefix}: {FormatDuration(Math.Max(0, _remainingSeconds))}";
            StatusLabel.TextColor = prefix == "Time is up" ? Colors.Red : Colors.Black;
            return;
        }

        if (_alarmPlaying)
        {
            StatusLabel.Text = $"Time is up: {FormatDuration(0)}";
            StatusLabel.TextColor = Colors.Red;
            return;
        }

        StatusLabel.Text = $"Remaining: {FormatDuration(Math.Max(0, _remainingSeconds))}";
        StatusLabel.TextColor = Colors.Black;
    }

    private bool TryGetEnteredDuration(out int durationSeconds)
    {
        // Start only when the entry contains a positive valid duration.
        return TryParseDuration(DurationEntry.Text, out durationSeconds) && durationSeconds > 0;
    }

    private static string SanitizeDurationInput(string? text)
    {
        // Keep only digits and a single colon to support seconds or m:ss input.
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(text.Length);
        var colonAdded = false;

        foreach (var character in text)
        {
            if (char.IsDigit(character))
            {
                builder.Append(character);
                continue;
            }

            if (character == ':' && !colonAdded)
            {
                builder.Append(character);
                colonAdded = true;
            }
        }

        return builder.ToString();
    }

    private static bool TryParseDuration(string? text, out int totalSeconds)
    {
        // Accept either raw seconds, for example 90, or m:ss, for example 1:30.
        totalSeconds = 0;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        text = text.Trim();

        if (int.TryParse(text, out var seconds) && seconds >= 0)
        {
            totalSeconds = seconds;
            return true;
        }

        var parts = text.Split(':');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out var minutes) && minutes >= 0 &&
            int.TryParse(parts[1], out var secs) && secs >= 0 && secs < 60)
        {
            totalSeconds = (minutes * 60) + secs;
            return true;
        }

        return false;
    }

    private static string FormatDuration(int totalSeconds)
    {
        // Format the countdown as m:ss for display.
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;
        return $"{minutes}:{seconds:00}";
    }
}
