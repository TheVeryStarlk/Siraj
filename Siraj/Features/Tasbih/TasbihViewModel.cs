using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Siraj.Features.Shell.Items;

namespace Siraj.Features.Tasbih;

internal sealed partial class TasbihViewModel : FooterViewModel
{
    [ObservableProperty]
    public partial int Value { get; set; }

    [ObservableProperty]
    public partial string Current { get; set; }

    private int index;

    private readonly string[] messages =
    [
        "سُبْحَانَ اللَّهِ",
        "سُبْحَانَ اللَّهِ وَبِحَمْدِهِ",
        "سُبْحَانَ اللَّهِ العَظِيمِ",
        "لا حَوْلَ وَلا قُوَّةَ إِلا بِاللَّهِ",
        "الْلَّهُم صَلِّ وَسَلِم على مُحَمَّد",
        "أستغفر الله",
        "الْلَّهُ أَكْبَرُ"
    ];

    public TasbihViewModel()
    {
        Glyph = "\uE8EF";
        Title = "Tasbih";

        Current = messages[index];
    }

    [RelayCommand]
    private void Add()
    {
        Value++;

        if (Value > 100)
        {
            Value = 0;
        }
    }

    [RelayCommand]
    private void Reset()
    {
        Value = 0;
    }

    [RelayCommand]
    private void Left()
    {
        index--;

        if (index < 1)
        {
            index = messages.Length - 1;
        }

        Current = messages[index];
    }

    [RelayCommand]
    private void Right()
    {
        index++;

        if (index > messages.Length - 1)
        {
            index = 0;
        }

        Current = messages[index];
    }
}