namespace repeat_timer_maui;

using System.Text;

public partial class MainPage : ContentPage
{
    private int? _durationSeconds;

    public MainPage()
    {
        InitializeComponent();
        SetDurationText("4");
    }

    private void OnDurationEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        var sanitizedText = SanitizeDurationInput(e.NewTextValue);
        if (sanitizedText != e.NewTextValue)
        {
            SetDurationText(sanitizedText);
            return;
        }

        UpdateDurationState(sanitizedText);
    }

    private void SetDurationText(string? text)
    {
        if (DurationEntry.Text != text)
        {
            DurationEntry.Text = text;
            return;
        }

        UpdateDurationState(text);
    }

    private void UpdateDurationState(string? text)
    {
        if (TryParseDuration(text, out var totalSeconds))
        {
            _durationSeconds = totalSeconds;
            DurationHintLabel.Text = $"Parsed: {totalSeconds} second(s) ({FormatDuration(totalSeconds)})";
            DurationHintLabel.TextColor = Colors.Green;
            return;
        }

        _durationSeconds = null;
        DurationHintLabel.Text = "Enter seconds or mm:ss";
        DurationHintLabel.TextColor = Colors.Gray;
    }

    private static string SanitizeDurationInput(string? text)
    {
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
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;
        return $"{minutes}:{seconds:00}";
    }
}
