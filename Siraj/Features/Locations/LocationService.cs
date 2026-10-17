using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Serilog;

namespace Siraj.Features.Locations;

internal sealed class LocationService(IHttpClientFactory clientFactory)
{
    private const string Url = "https://nominatim.openstreetmap.org/";

    public async Task<Location[]> SearchAsync(string query)
    {
        Log.Debug("Searching for locations matching {Query}", query);

        try
        {
            var client = clientFactory.CreateClient();

            client.DefaultRequestHeaders.Add("User-Agent", nameof(Siraj));
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US");

            var result = await client.GetFromJsonAsync(
                $"{Url}search?q={Uri.EscapeDataString(query)}&format=jsonv2",
                SirajSerializerContext.Default.LocationArray);

            ArgumentNullException.ThrowIfNull(result);

            Log.Debug("Location search returned {LocationCount} results", result.Length);

            return result;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Location search failed for {Query}", query);
            throw;
        }
    }

    public async Task<Location> ReverseAsync(double latitude, double longitude)
    {
        Log.Debug("Reverse geocoding the current position");

        try
        {
            var client = clientFactory.CreateClient();

            client.DefaultRequestHeaders.Add("User-Agent", nameof(Siraj));
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US");

            var result = await client.GetFromJsonAsync(
                $"{Url}reverse?lat={latitude}&lon={longitude}&zoom=18&format=jsonv2",
                SirajSerializerContext.Default.Location);

            ArgumentNullException.ThrowIfNull(result);

            Log.Debug("Reverse geocoding resolved to {LocationName}", result.Name);

            return result;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Reverse geocoding failed");
            throw;
        }
    }
}