using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Siraj.Features.Settings;
using Siraj.Features.Shell.Items;

namespace Siraj.Features.Tasbih;

internal sealed partial class TasbihViewModel : FooterViewModel
{
    public int Total => messages.Length;

    [ObservableProperty]
    public partial int Index { get; set; }

    [ObservableProperty]
    public partial int Count { get; set; }

    [ObservableProperty]
    public partial int Value { get; set; }

    [ObservableProperty]
    public partial string Rounds { get; set; }

    [ObservableProperty]
    public partial string Current { get; set; }

    private const int Round = 33;

    private readonly string[] messages =
    [
        "سُبْحَانَ اللَّهِ",
        "الْحَمْدُ لِلَّهِ",
        "اللَّهُ أَكْبَرُ",
        "سُبْحَانَ اللَّهِ وَبِحَمْدِهِ",
        "سُبْحَانَ اللَّهِ الْعَظِيمِ",
        "لَا حَوْلَ وَلَا قُوَّةَ إِلَّا بِاللَّهِ",
        "اللَّهُمَّ صَلِّ وَسَلِّمْ عَلَى مُحَمَّدٍ",
        "أَسْتَغْفِرُ اللَّهَ"
    ];

    private readonly int[] counts;

    private readonly SettingsService settingsService;

    public TasbihViewModel(SettingsService settingsService)
    {
        this.settingsService = settingsService;

        Glyph = "";
        Title = "Tasbih";

        counts = settingsService.Counts;
        Array.Resize(ref counts, messages.Length);

        Index = Math.Clamp(settingsService.Dhikr, 0, messages.Length - 1);
        Current = messages[Index];
        Rounds = string.Empty;

        Update();
    }

    partial void OnIndexChanged(int value)
    {
        // The pips pager can briefly report -1 while it initializes.
        if (value < 0 || value >= messages.Length)
        {
            return;
        }

        Current = messages[value];
        settingsService.Dhikr = value;

        Update();
    }

    [RelayCommand]
    private void Add()
    {
        counts[Index]++;
        settingsService.Counts = counts;

        Update();
    }

    [RelayCommand]
    private void Reset()
    {
        counts[Index] = 0;
        settingsService.Counts = counts;

        Update();
    }

    [RelayCommand]
    private void Left()
    {
        Index = (Index - 1 + messages.Length) % messages.Length;
    }

    [RelayCommand]
    private void Right()
    {
        Index = (Index + 1) % messages.Length;
    }

    private void Update()
    {
        Count = counts[Index];

        // Fill the gauge completely on every 33rd count before starting over.
        Value = Count is 0 ? 0 : (Count - 1) % Round + 1;
        Rounds = Count > Round ? $"Round {(Count - 1) / Round + 1}" : string.Empty;
    }
}
