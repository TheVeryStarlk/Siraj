using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Siraj.Features.Locations;
using Siraj.Features.Prayers.Calculation;
using Siraj.Features.Prayers.Voices;
using Siraj.Features.Settings;
using Siraj.Features.Shell.Items;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Serilog;

namespace Siraj.Features.Prayers;

internal sealed partial class PrayersViewModel : HeaderViewModel, IDisposable
{
    public override bool IsDeletable => true;

    public Location Location { get; }

    public ObservableCollection<Prayer> Prayers { get; } = [];

    public bool IsNotified
    {
        get => Location.IsNotified;
        set => SetProperty(Location.IsNotified, value, Location, static (location, notified) => location.IsNotified = notified);
    }

    [ObservableProperty]
    public partial string? Description { get; set; }

    [ObservableProperty]
    public partial PrayerKind? Upcoming { get; set; }

    [ObservableProperty]
    public partial string? Remaining { get; set; }

    private DateTime day;
    private DateTimeOffset tomorrow;
    private DateTimeOffset last;

    private static readonly CultureInfo Hijri = new("ar-SA");

    private readonly SettingsService settingsService;
    private readonly NotificationService notificationService;
    private readonly TimerService timerService;
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
        this.timerService = timerService;
        this.voiceService = voiceService;
        this.timeProvider = timeProvider;

        Location = location;
        Title = location.Name;

        last = timeProvider.GetLocalNow().TruncateToMinute();

        timerService.Start();
        timerService.Tick += Refresh;

        WeakReferenceMessenger.Default.Register<PrayersViewModel, MethodChangedMessage>(this, static (recipient, _) => recipient.Initialize());

        Initialize();
    }

    private void Initialize()
    {
        Calculate(timeProvider.GetLocalNow());
        Refresh();
    }

    private void Calculate(DateTimeOffset now)
    {
        Log.Information("Calculating prayer times for {LocationName}", Location.Name);

        var calculator = new PrayerTimesCalculator(settingsService.Method.ToOptions());
        var times = calculator.Calculate(now, Location.Latitude, Location.Longitude);

        Prayers.Clear();

        foreach (var kind in Enum.GetValues<PrayerKind>())
        {
            Prayers.Add(new Prayer(kind, times[kind]));
        }

        day = now.Date;
        tomorrow = calculator.Calculate(now.AddDays(1), Location.Latitude, Location.Longitude)[PrayerKind.Fajr];
        Description = now.ToString(CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern, Hijri);
    }

    [RelayCommand]
    private void Refresh()
    {
        var now = timeProvider.GetLocalNow().TruncateToMinute();
        var reached = now != last ? Prayers.LastOrDefault(prayer => prayer.Time == now) : null;

        last = now;

        if (now.Date != day && now > Prayers[^1].Time)
        {
            Calculate(now);
        }

        var upcoming = Prayers.FirstOrDefault(prayer => prayer.Time > now) ?? new Prayer(PrayerKind.Fajr, tomorrow);

        Glyph = upcoming.Emoji;
        Upcoming = upcoming.Kind;
        Remaining = (upcoming.Time - now).ToReadableString();

        if (reached is null || !IsNotified)
        {
            return;
        }

        Log.Information("Prayer time reached for {Prayer} at {LocationName}", reached.Kind, Location.Name);

        notificationService.Show($"Now is the prayer time for {reached.Kind}", Location.Name, upcoming.Time);
        voiceService.Play(settingsService.Voice, reached.Kind);
    }

    public void Dispose()
    {
        timerService.Tick -= Refresh;
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}

internal sealed class Prayer(PrayerKind kind, DateTimeOffset time)
{
    public PrayerKind Kind => kind;

    public DateTimeOffset Time { get; } = time.TruncateToMinute();

    public string When => Time.ToLocalTime().ToString("h:mm tt");

    public string Emoji => kind switch
    {
        PrayerKind.Fajr => "🌅",
        PrayerKind.Dhuhr => "🌄",
        PrayerKind.Asr => "🌇",
        PrayerKind.Maghrib => "🌆",
        PrayerKind.Isha => "🌌",
        _ => throw new ArgumentOutOfRangeException()
    };
}