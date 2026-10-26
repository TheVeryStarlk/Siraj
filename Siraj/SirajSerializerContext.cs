using System.Text.Json.Serialization;
using Siraj.Features.Locations;
using Siraj.Features.Prayers.Calculation;
using Siraj.Features.Prayers.Voices;
using Siraj.Features.Settings;

namespace Siraj;

[JsonSourceGenerationOptions(NumberHandling = JsonNumberHandling.AllowReadingFromString)]
[JsonSerializable(typeof(Location[]))]
[JsonSerializable(typeof(Theme))]
[JsonSerializable(typeof(Voice))]
[JsonSerializable(typeof(Method))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(int[]))]
internal sealed partial class SirajSerializerContext : JsonSerializerContext;