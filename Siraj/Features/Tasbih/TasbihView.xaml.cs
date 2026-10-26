using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Serilog;

namespace Siraj.Features.Tasbih;

internal sealed partial class TasbihView : Page
{
    public TasbihViewModel ViewModel
    {
        get
        {
            ArgumentNullException.ThrowIfNull(field);
            return field;
        }
        set;
    }

    public TasbihView()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
    {
        ViewModel = (TasbihViewModel) eventArgs.Parameter;

        Log.Debug("Opened Tasbih view");

        base.OnNavigatedTo(eventArgs);
    }

    private void PageLoaded(object sender, RoutedEventArgs eventArgs)
    {
        // Focus the counter so that the keyboard can be used right away.
        CounterButton.Focus(FocusState.Programmatic);
    }

    private void ResetClicked(object sender, RoutedEventArgs eventArgs)
    {
        ResetFlyout.Hide();
        CounterButton.Focus(FocusState.Programmatic);
    }
}