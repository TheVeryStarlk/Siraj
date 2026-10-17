using System;
using System.Text.Json.Serialization;

namespace Siraj.Features.Locations;

internal sealed class Location
{
    [JsonPropertyName("display_name")]
    public required string Name { get; init; }

    [JsonPropertyName("lat")]
    public required double Latitude { get; init; }

    [JsonPropertyName("lon")]
    public required double Longitude { get; init; }

    public bool IsNotified { get; set; }

    public bool Equals(Location instance)
    {
        const float tolerance = 0.1F;
        
        return Math.Abs(Latitude - instance.Latitude) < tolerance && Math.Abs(Longitude - instance.Longitude) < tolerance;
    }
}