using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Siraj.Features.Prayers.Calculation;
using Serilog;
using System;
using Windows.Foundation;
using Windows.UI;

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

        var (red, blue, green) = ViewModel.Upcoming.Value switch
        {
            PrayerKind.Fajr => (255, 239, 120),
            PrayerKind.Dhuhr => (255, 208, 120),
            PrayerKind.Asr => (245, 210, 144),
            PrayerKind.Maghrib => (102, 115, 255),
            PrayerKind.Isha => (83, 57, 250),
            _ => throw new ArgumentOutOfRangeException()
        };

        GradientRectangle.Fill = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, 1),
            GradientStops =
            {
                new GradientStop
                {
                    Offset = 0,
                    Color = Color.FromArgb(50, (byte) red, (byte) blue, (byte) green)
                },
                new GradientStop
                {
                    Offset = 1,
                    Color = Color.FromArgb(0, (byte) red, (byte) blue, (byte) green)
                }
            }
        };

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