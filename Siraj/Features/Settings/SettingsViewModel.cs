using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Threading.Tasks;
using Siraj.Features.Prayers.Calculation;
using Siraj.Features.Shell.Items;
using Siraj.Features.Prayers.Voices;

namespace Siraj.Features.Settings;

internal sealed partial class SettingsViewModel : FooterViewModel
{
    [ObservableProperty]
    public partial Theme Theme { get; set; }

    [ObservableProperty]
    public partial Voice Voice { get; set; }

    [ObservableProperty]
    public partial Method Method { get; set; }

    [ObservableProperty]
    public partial bool Startup { get; set; }

    private readonly SettingsService settingsService;
    private readonly ThemeService themeService;
    private readonly StartupService startupService;
    private readonly LauncherService launcherService;

    public SettingsViewModel(SettingsService settingsService, ThemeService themeService, StartupService startupService, LauncherService launcherService)
    {
        this.settingsService = settingsService;
        this.themeService = themeService;
        this.startupService = startupService;
        this.launcherService = launcherService;

        Glyph = "\uE713";
        Title = "Settings";
    }

    [RelayCommand]
    private void Initialize()
    {
        Theme = settingsService.Theme;
        Voice = settingsService.Voice;
        Method = settingsService.Method;
        Startup = settingsService.Startup;
    }

    [RelayCommand]
    private async Task OpenLogFileAsync()
    {
        await launcherService.OpenLogsAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (await startupService.TryToggle(Startup))
        {
            settingsService.Startup = Startup;
        }
    }

    partial void OnThemeChanged(Theme value)
    {
        settingsService.Theme = value;
        themeService.Set(value);
    }

    partial void OnVoiceChanged(Voice value)
    {
        settingsService.Voice = value;
    }

    partial void OnMethodChanged(Method value)
    {
        if (value == settingsService.Method)
        {
            return;
        }

        settingsService.Method = value;
        WeakReferenceMessenger.Default.Send(new MethodChangedMessage());
    }
}