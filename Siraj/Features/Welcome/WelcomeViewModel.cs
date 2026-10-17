using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using System;
using CommunityToolkit.Mvvm.Messaging;
using Siraj.Features.Locations;
using Siraj.Features.Shell.Items;
using Serilog;

namespace Siraj.Features.Welcome;

internal sealed partial class WelcomeViewModel : HeaderViewModel
{
    private readonly DialogService dialogService;
    private readonly GeopositionService geopositionService;
    private readonly LocationService locationService;

    public WelcomeViewModel(
        DialogService dialogService,
        GeopositionService geopositionService,
        LocationService locationService)
    {
        this.dialogService = dialogService;
        this.geopositionService = geopositionService;
        this.locationService = locationService;

        Title = "Welcome";
        Glyph = "🚀";
    }

    [RelayCommand]
    private async Task LocateAsync()
    {
        Log.Information("Starting location resolve");

        try
        {
            if (!await geopositionService.IsAllowedAsync())
            {
                Log.Warning("Location permission was not granted");

                await dialogService.ShowMessageAsync("Where are you?", "Make sure location access is enabled in your system");

                return;
            }

            var geoposition = await geopositionService.GetAsync();
            var location = await locationService.ReverseAsync(geoposition.Latitude, geoposition.Longitude);

            Log.Information("Location resolved to {LocationName}", location.Name);

            location.IsNotified = true;

            WeakReferenceMessenger.Default.Send(new AddMessage(location));
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Location resolving failed");
            throw;
        }
    }
}