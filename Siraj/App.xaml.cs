using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Serilog;
using Siraj.Features.Settings;
using Siraj.Features.Shell;
using System;
using System.Threading.Tasks;

namespace Siraj;

public sealed partial class App : Application
{
    public App()
    {
        InitializeComponent();

        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) => Log.Fatal(eventArgs.ExceptionObject as Exception, "Siraj terminated unexpectedly");
        TaskScheduler.UnobservedTaskException += (_, eventArgs) => Log.Fatal(eventArgs.Exception, "Siraj terminated unexpectedly");
        UnhandledException += (_, eventArgs) => Log.Fatal(eventArgs.Exception, "Siraj terminated unexpectedly");
    }

    protected override void OnLaunched(LaunchActivatedEventArgs eventArgs)
    {
        var settingsService = Bootstrapper.Services.GetRequiredService<SettingsService>();
        var themeService = Bootstrapper.Services.GetRequiredService<ThemeService>();
        var shellView = Bootstrapper.Services.GetRequiredService<ShellView>();

        themeService.Set(settingsService.Theme);
        shellView.Activate();
    }
}