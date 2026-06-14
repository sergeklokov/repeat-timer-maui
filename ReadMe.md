# Repeat Timer MAUI — Android Build Guide

This document explains how to create the Repeat Timer application using .NET MAUI, optimized for Android deployment. It includes project setup, timer logic structure, sound playback, UI layout, and packaging for APK/AAB.

![Repeat Timer MAUI](02%20repeat-timer-maui%20ready.png)

## 1. Prerequisites

### Required Software

- **Visual Studio 2022** (17.8 or later)
- **Workloads**:
  - .NET Multi-platform App UI development
  - Android development
- **.NET 8 SDK**
- **Android SDK + Emulator** (installed via Visual Studio Installer)

### Optional

- Physical Android device with USB debugging enabled
- MAUI Community Toolkit (for additional UI helpers)

## 2. Create the MAUI Project

### Command Line

```bash
dotnet new maui -n RepeatTimerMaui
cd RepeatTimerMaui
```

### Visual Studio

1. Create a new project
2. Select **.NET MAUI App**
3. Name it: `RepeatTimerMaui`
4. Choose **.NET 8**

## 3. Project Structure

```
RepeatTimerMaui/
├── Platforms/
│   └── Android/
├── Resources/
│   └── Raw/
│       └── beep.mp3
├── MainPage.xaml
├── MainPage.xaml.cs
└── TimerService.cs
```

## 4. Add the Beep Sound

Place your sound file here:

```
Resources/Raw/beep.mp3
```

MAUI automatically includes it in the Android build.

## 5. Implement Timer Logic

Create a file: `TimerService.cs`

```csharp
public class TimerService
{
    private CancellationTokenSource _cts;

    public async Task StartTimerAsync(int minutes, Action onTick, Action onFinished)
    {
        _cts = new CancellationTokenSource();
        int totalSeconds = minutes * 60;

        while (totalSeconds > 0 && !_cts.IsCancellationRequested)
        {
            await Task.Delay(1000);
            totalSeconds--;
            onTick?.Invoke();
        }

        if (!_cts.IsCancellationRequested)
            onFinished?.Invoke();
    }

    public void Stop() => _cts?.Cancel();
}
```

## 6. UI Layout (MainPage.xaml)

```xml
<VerticalStackLayout Padding="20" Spacing="15">
    <Label x:Name="TimerLabel" FontSize="48" HorizontalOptions="Center" />

    <Button Text="Start" Clicked="OnStartClicked" />
    <Button Text="Next +1 min" Clicked="OnNextPlusClicked" />
    <Button Text="Next -1 min" Clicked="OnNextMinusClicked" />
    <Button Text="Done" Clicked="OnDoneClicked" />
</VerticalStackLayout>
```

## 7. UI Logic (MainPage.xaml.cs)

```csharp
public partial class MainPage : ContentPage
{
    private int _minutes = 8;
    private readonly TimerService _timer = new();

    public MainPage()
    {
        InitializeComponent();
        UpdateLabel();
    }

    private void UpdateLabel() => TimerLabel.Text = $"{_minutes} min";

    private async void OnStartClicked(object sender, EventArgs e)
    {
        await _timer.StartTimerAsync(_minutes, UpdateLabel, PlayBeep);
    }

    private void OnNextPlusClicked(object sender, EventArgs e)
    {
        _minutes++;
        UpdateLabel();
    }

    private void OnNextMinusClicked(object sender, EventArgs e)
    {
        if (_minutes > 1) _minutes--;
        UpdateLabel();
    }

    private void OnDoneClicked(object sender, EventArgs e)
    {
        _timer.Stop();
    }

    private async void PlayBeep()
    {
        var player = AudioManager.Current.CreatePlayer(
            await FileSystem.OpenAppPackageFileAsync("beep.mp3"));
        player.Play();
    }
}
```

## 8. Configure Android Manifest

Edit: `Platforms/Android/AndroidManifest.xml`

Add permissions if needed:

```xml
<uses-permission android:name="android.permission.VIBRATE" />
```

## 9. Build & Deploy to Android

### Debug on Device

- Connect phone → enable USB debugging
- In Visual Studio: select Android Device → Run

### Build APK

In Visual Studio:
- **Build → Publish → Android → APK**

### Build AAB (Play Store)

- **Build → Publish → Android → AAB**

## 10. Summary

You now have:

- A working Repeat Timer MAUI app
- Adjustable timer (+1 / –1 minute)
- Endless repeat until "Done"
- Beep sound on each cycle
- Android-ready APK/AAB packaging

This structure is clean, scalable, and easy to extend.


## 11. Motorola Device Compatibility

### Moto 2025 Series (e.g. Moto G 2025, Moto Edge 2025)
- **Shipping OS**: Android 15
- **Update status**: Receiving Android 16 updates
- **Recommended MAUI Settings**:
  - Target SDK: **Android 15 (API 35)**
  - Minimum SDK: Android 10+

### Moto Stylus 5G 2022 (Moto G Stylus 5G 2022)
- **Original OS**: Android 12
- **Last major update**: **Android 13**
- **Current version**: Android 13 (with security patches until early 2025)
- **Android 14 / 15 support**: Not available
- **Recommended MAUI Settings**:
  - Target SDK: **Android 13 (API 33)**
  - Minimum SDK: Android 8.0 (API 26) or higher for broader compatibility

### General Recommendation for Repeat Timer MAUI App

To support **both new 2025 Motorola phones and older devices** like the Moto Stylus 5G 2022:

1. Open `Platforms/Android/AndroidManifest.xml`
2. Set the target and minimum SDK versions:

```xml
<uses-sdk android:minSdkVersion="26" android:targetSdkVersion="35" />
```

In your .csproj file, ensure you have:

```xml
<TargetFrameworks>net8.0-android</TargetFrameworks>
<SupportedOSPlatformVersion>26</SupportedOSPlatformVersion>
```

This configuration gives you:

Good performance on new Moto 2025 phones (Android 15/16)
Full compatibility with older phones like Moto Stylus 5G 2022 (Android 13)

## 12. Prompt for AI Code Generation
Based on this app, genereated from template, I want to create "repeat timer" app for Android. I should have field to input duration in seconds and buttons Start, Next, Increase 10%, Decrease 10%, Done.
When start, timer start working until time gone, then play sound forever. If user click button Next it repeats. Increase/Decrease buttons add/remove 10% time based on intial time. Done stop it. Update code for it.

12.1. Remove items created by template from MainPage.  Add fields related  to timer: input duration in seconds and buttons Start, Next, Increase 10%, Decrease 10%, Done.

12.2. Add default time 4 sec, also allow only digits and ":" sign for entering

12.3. Current timer duration set 4 sec for debugging purposes. If I want t ochange it to 480 sec, then how to do it? How to make it a such way, so this change happens in one place of code/form?

12.4. Add setting file, move default duration configuration to it.

12.5. Change status color to white. 

12.6. Rename StatusLabel to Remaining label. Don't change f-ing color in runtime, we have xaml for it!
