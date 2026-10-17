using System;
using CommunityToolkit.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Siraj.Features.Locations;
using Siraj.Features.Prayers;
using Siraj.Features.Prayers.Voices;
using Siraj.Features.Settings;
using Siraj.Features.Shell;
using Siraj.Features.Tasbih;
using Siraj.Features.Welcome;

namespace Siraj;

internal static partial class Bootstrapper
{
    public static ServiceProvider Services { get; } = Build();

    private static ServiceProvider Build()
    {
        var services = new ServiceCollection();

        services.AddHttpClient();

#if DEBUG
        services.AddSingleton<TimeProvider, DebugTimeProvider>();
#else
        services.AddSingleton(TimeProvider.System);
#endif

        Configure(services);
        ConfigureViews(services);
        ConfigureViewModels(services);

        return services.BuildServiceProvider();
    }

    // Locations.
    [Transient(typeof(LocationService))]

    // Prayers.
    [Singleton(typeof(NotificationService))]
    [Singleton(typeof(TimerService))]
    [Singleton(typeof(VoiceService))]

    // Settings.
    [Singleton(typeof(SettingsService))]
    [Transient(typeof(LauncherService))]
    [Transient(typeof(StartupService))]
    [Transient(typeof(ThemeService))]

    // Shell.
    [Singleton(typeof(NavigationService), typeof(INavigationService))]
    [Singleton(typeof(WindowService))]

    // Welcome.
    [Transient(typeof(DialogService))]
    [Transient(typeof(GeopositionService))]
    private static partial void Configure(IServiceCollection services);

    [Singleton(typeof(ShellView))]
    private static partial void ConfigureViews(IServiceCollection services);

    [Singleton(typeof(PrayersViewModelFactory))]
    [Singleton(typeof(SettingsViewModel))]
    [Transient(typeof(ShellViewModel))]
    [Transient(typeof(TasbihViewModel))]
    [Transient(typeof(WelcomeViewModel))]
    private static partial void ConfigureViewModels(IServiceCollection services);
}

internal sealed class DebugTimeProvider : TimeProvider
{
    public override DateTimeOffset GetUtcNow()
    {
        return DateTimeOffset.UtcNow;
    }
}