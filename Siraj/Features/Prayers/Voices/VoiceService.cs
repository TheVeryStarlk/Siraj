using System;
using Windows.Media.Core;
using Windows.Media.Playback;
using Serilog;
using Siraj.Features.Prayers.Calculation;

namespace Siraj.Features.Prayers.Voices;

internal sealed class VoiceService
{
    private readonly MediaPlayer player = new()
    {
        AudioCategory = MediaPlayerAudioCategory.Alerts
    };

    public void Play(Voice voice, PrayerKind prayer)
    {
        if (player.CurrentState is MediaPlayerState.Playing)
        {
            return;
        }

        var suffix = prayer is PrayerKind.Fajr ? nameof(PrayerKind.Fajr) : string.Empty;
        var source = $"{voice}{suffix}.mp3";

        player.Source = MediaSource.CreateFromUri(new Uri($"ms-appx:///Assets/Voices/{source}"));

        player.Play();

        Log.Information("Playing prayer audio {AudioFile}", suffix);
    }
}