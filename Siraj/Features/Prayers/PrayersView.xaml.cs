using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Serilog;
using System;

namespace Siraj.Features.Prayers;

internal sealed partial class PrayersView : Page
{
    public PrayersViewModel ViewModel
    {
        get
        {
            ArgumentNullException.ThrowIfNull(field);
            return field;
        }
        set;
    }

    public PrayersView()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
    {
        ViewModel = (PrayersViewModel) eventArgs.Parameter;

        Log.Debug("Opened prayers view for {LocationName}", ViewModel.Location.Name);

        ViewModel.InitializeCommand.Execute(null);

        base.OnNavigatedTo(eventArgs);
    }

    // I don't like this.
    private void GradientRectangleLoaded(object sender, RoutedEventArgs eventArgs)
    {
        if (!ViewModel.Upcoming.HasValue)
        {
            return;
        }

        GradientRectangle.Fill = (Brush) Resources[ViewModel.Upcoming.Value.ToString()];

        var visual = ElementCompositionPreview.GetElementVisual(GradientRectangle);
        var compositor = visual.Compositor;

        var easing = CompositionEasingFunction.CreatePowerEasingFunction(
            compositor,
            CompositionEasingFunctionMode.Out,
            5);

        var animation = compositor.CreateScalarKeyFrameAnimation();

        animation.InsertKeyFrame(0, 0);
        animation.InsertKeyFrame(1, 1, easing);
        animation.Duration = TimeSpan.FromSeconds(2.5);

        visual.StartAnimation(nameof(Visual.Opacity), animation);
    }
}