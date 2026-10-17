using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Siraj.Features.Locations;
using Siraj.Features.Prayers.Calculation;
using Siraj.Features.Prayers.Voices;
using Siraj.Features.Settings;
using Siraj.Features.Shell.Items;
using System;
using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Serilog;

namespace Siraj.Features.Prayers;

internal sealed partial class PrayersViewModel : HeaderViewModel
{
    public override bool IsDeletable => true;

    public Location Location { get; }

    public ObservableCollection<Prayer> Prayers { get; } = [];

    [ObservableProperty]
    public partial bool IsNotified { get; set; }

    [ObservableProperty]
    public partial string? Description { get; set; }

    [ObservableProperty]
    public partial PrayerKind? Upcoming { get; set; }

    [ObservableProperty]
    public partial string? Remaining { get; set; }

    private FrozenDictionary<PrayerKind, DateTimeOffset>? times;

    private readonly SettingsService settingsService;
    private readonly NotificationService notificationService;
    private readonly VoiceService voiceService;
    private readonly TimeProvider timeProvider;

    public PrayersViewModel(
        SettingsService settingsService,
        NotificationService notificationService,
        TimerService timerService,
        VoiceService voiceService,
        TimeProvider timeProvider,
        Location location)
    {
        this.settingsService = settingsService;
        this.notificationService = notificationService;
        this.voiceService = voiceService;
        this.timeProvider = timeProvider;

        Location = location;
        Title = location.Name;

        timerService.Start();
        timerService.Tick += Refresh;

        Initialize();
    }

    [RelayCommand]
    private void Initialize()
    {
        Log.Information("Initializing prayer times for {LocationName}", Location.Name);

        IsNotified = Location.IsNotified;

        Prayers.Clear();

        var now = timeProvider.GetLocalNow();

        Description = now.ToString(CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern, new CultureInfo("ar-SA"));

        var calculator = new PrayerTimesCalculator(settingsService.Method.ToOptions());

        times = calculator.Calculate(now, Location.Latitude, Location.Longitude);

        Log.Debug("Calculated {PrayerCount} prayer times for {LocationName}", times.Count, Location.Name);

        foreach (var pair in times)
        {
            Prayers.Add(new Prayer(pair.Key, pair.Value.ToLocalTime().ToString("h:mm tt")));
        }

        // Force a refresh; Instead of waiting for a minute to refresh.
        Refresh();
    }

    private void Refresh()
    {
        if (times is null)
        {
            return;
        }

        var now = timeProvider.GetLocalNow();

        times.FirstOrDefault(pair => pair.Value > now).Deconstruct(out var kind, out var time);

        // Show how much is left for Fajr in the next day.
        if (time == default)
        {
            time = times[PrayerKind.Fajr].AddDays(1);
        }

        if (IsNotified && Upcoming != kind && !string.IsNullOrWhiteSpace(Remaining))
        {
            Log.Information("Prayer time reached for {Prayer} at {LocationName}", Upcoming, Location.Name);

            Debug.Assert(Upcoming.HasValue);
            
            notificationService.Show($"Now is the prayer time for {Upcoming}", Location.Name);
            voiceService.Play(settingsService.Voice, Upcoming.Value);
        }

        Glyph = Prayer.ToEmoji(kind);

        Upcoming = kind;
        Remaining = (time - now).ToReadableString();
    }

    partial void OnIsNotifiedChanged(bool value)
    {
        Log.Information("Turned notifications {State} for {LocationName}", value ? "on" : "off", Location.Name);
        Location.IsNotified = value;
    }
}

internal sealed class Prayer(PrayerKind kind, string time)
{
    public PrayerKind Kind => kind;

    public string Time => time;

    public string Emoji => ToEmoji(kind);

    public static string ToEmoji(PrayerKind kind)
    {
        return kind switch
        {
            PrayerKind.Fajr => "🌅",
            PrayerKind.Dhuhr => "🌄",
            PrayerKind.Asr => "🌇",
            PrayerKind.Maghrib => "🌆",
            PrayerKind.Isha => "🌌",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}