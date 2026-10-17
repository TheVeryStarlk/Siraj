using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Siraj.Features.Locations;
using Siraj.Features.Prayers;
using Siraj.Features.Settings;
using Siraj.Features.Shell.Items;
using Siraj.Features.Tasbih;
using Siraj.Features.Welcome;
using Serilog;

namespace Siraj.Features.Shell;

internal sealed partial class ShellViewModel : ObservableObject
{
    public ObservableCollection<string> Suggestions { get; } = [];

    public ObservableCollection<HeaderViewModel> Header { get; } = [];

    public ObservableCollection<FooterViewModel> Footer { get; }

    [ObservableProperty]
    public partial ItemViewModel? Current { get; set; }

    private Location[]? locations;

    private readonly INavigationService navigationService;
    private readonly LocationService locationService;
    private readonly WindowService windowService;
    private readonly SettingsService settingsService;
    private readonly NotificationService notificationService;
    private readonly PrayersViewModelFactory prayersViewModelFactory;
    private readonly WelcomeViewModel welcomeViewModel;

    public ShellViewModel(
        INavigationService navigationService,
        LocationService locationService,
        WindowService windowService,
        SettingsService settingsService,
        NotificationService notificationService,
        PrayersViewModelFactory prayersViewModelFactory,
        TasbihViewModel tasbihViewModel,
        WelcomeViewModel welcomeViewModel,
        SettingsViewModel settingsViewModel)
    {
        this.navigationService = navigationService;
        this.settingsService = settingsService;
        this.notificationService = notificationService;
        this.prayersViewModelFactory = prayersViewModelFactory;
        this.welcomeViewModel = welcomeViewModel;
        this.locationService = locationService;
        this.windowService = windowService;

        Footer =
        [
            tasbihViewModel,
            settingsViewModel
        ];

        WeakReferenceMessenger.Default.Register<ShellViewModel, AddMessage>(this, Add);
        WeakReferenceMessenger.Default.Register<ShellViewModel, DeleteMessage>(this, Delete);
    }

    private static void Add(ShellViewModel recipient, AddMessage message)
    {
        Log.Information("Adding location {LocationName}", message.Location.Name);

        if (recipient.Header[0] is WelcomeViewModel)
        {
            recipient.Header.RemoveAt(0);
        }

        var instance = recipient.prayersViewModelFactory.Create(message.Location);

        recipient.Header.Add(instance);
        recipient.Navigate(recipient.Header.Last());
    }

    private static void Delete(ShellViewModel recipient, DeleteMessage message)
    {
        Log.Information("Removing location {LocationName}", message.Instance.Title);

        recipient.Header.Remove(message.Instance);

        if (recipient.Header.Count is 0)
        {
            recipient.Header.Add(recipient.welcomeViewModel);
        }

        recipient.Navigate(recipient.Header[0]);
    }

    [RelayCommand]
    private void Initialize()
    {
        var saved = settingsService.Locations;

        Log.Information("Initializing shell with {LocationCount} saved locations", saved.Length);

        if (saved.Length is 0)
        {
            Header.Add(welcomeViewModel);
        }
        else
        {
            foreach (var location in saved)
            {
                Header.Add(prayersViewModelFactory.Create(location));
            }
        }

        Navigate(Header[^1]);
    }

    [RelayCommand]
    private async Task SearchAsync(string? input)
    {
        Suggestions.Clear();

        if (string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        Log.Debug("Searching for locations matching {Query}", input);

        locations = await locationService.SearchAsync(input);

        Log.Debug("Loaded {LocationCount} location suggestions", locations.Length);

        foreach (var location in locations)
        {
            Suggestions.Add(location.Name);
        }
    }

    [RelayCommand]
    private void Select(string? instance)
    {
        if (Suggestions.Count < 1)
        {
            return;
        }

        instance ??= Suggestions[0];

        Suggestions.Clear();

        var location = locations?.FirstOrDefault(location => location.Name.Equals(instance));

        if (location is null)
        {
            return;
        }

        Log.Information("Selected location {LocationName}", location.Name);

        if (Header[0] is WelcomeViewModel)
        {
            Header.RemoveAt(0);
        }

        Header.Add(prayersViewModelFactory.Create(location));

        Navigate(Header[^1]);
    }

    [RelayCommand]
    private void Navigate(ItemViewModel? selection)
    {
        if (Current == selection || selection is null)
        {
            return;
        }

        Current = selection;

        Log.Debug("Selected shell item {ItemTitle}", selection.Title);
        navigationService.Navigate(Current);
    }

    [RelayCommand]
    private void Open()
    {
        Log.Debug("Opening the shell window");
        windowService.Open();
    }

    [RelayCommand]
    private void Exit()
    {
        Log.Information("Exiting Siraj and saving locations");
        settingsService.Locations = Header.OfType<PrayersViewModel>().Select(header => header.Location).ToArray();
        windowService.Exit();
    }
    
    [RelayCommand]
    private void OnMinimized()
    {
        notificationService.Show("Siraj is running", "Siraj has been minimized to the tray icon");
    }
}