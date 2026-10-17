using System;
using System.Linq;
using Windows.Foundation;
using Windows.Graphics;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WinUIEx;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Siraj.Features.Shell.Items;
using Serilog;

namespace Siraj.Features.Shell;

internal sealed partial class ShellView : WindowEx
{
    private readonly ShellViewModel viewModel;
    private readonly DispatcherQueueTimer timer;

    public ShellView(ShellViewModel viewModel)
    {
        this.viewModel = viewModel;

        timer = DispatcherQueue.GetForCurrentThread().CreateTimer();

        timer.Tick += OnTick;
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.IsRepeating = false;

        InitializeComponent();
        SetTitleBar(TitleBar);

        this.CenterOnScreen();

        ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

        Closed += OnClosed;
    }

    private void OnClosed(object sender, WindowEventArgs eventArgs)
    {
        TrayIcon.Dispose();
        viewModel.ExitCommand.Execute(null);
    }

    private async void OnTick(DispatcherQueueTimer sender, object eventArgs)
    {
        try
        {
            await viewModel.SearchCommand.ExecuteAsync(SearchBox.Text);
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Location search failed for {Query}", SearchBox.Text);
        }
    }

    protected override void OnStateChanged(WindowState state)
    {
        if (state is WindowState.Minimized)
        {
            this.Hide();
            viewModel.MinimizedCommand.Execute(null);
        }

        NavigationView.Margin = WindowState is WindowState.Maximized ? new Thickness(0, -1, 0, 0) : new Thickness(0, -2, 0, 0);
    }

    // https://discord.com/channels/372137812037730304/671870147354427422/1492298194749620236
    private void TitleBarSizeChanged(object sender, SizeChangedEventArgs eventArgs)
    {
        var scale = TitleBar.XamlRoot.RasterizationScale;

        var elements = new FrameworkElement[]
        {
            ToggleButton,
            SearchBox
        };

        var rects = elements
            .Select(element =>
            {
                var transform = element.TransformToVisual(null);
                var bounds = transform.TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));

                return new RectInt32(
                    (int) Math.Round(bounds.X * scale),
                    (int) Math.Round(bounds.Y * scale),
                    (int) Math.Round(bounds.Width * scale),
                    (int) Math.Round(bounds.Height * scale)
                );
            })
            .ToArray();

        InputNonClientPointerSource.GetForWindowId(AppWindow.Id).SetRegionRects(NonClientRegionKind.Passthrough, rects);
    }

    private void ToggleButtonClick(object sender, RoutedEventArgs eventArgs)
    {
        NavigationView.IsPaneOpen = !NavigationView.IsPaneOpen;
    }

    private void SearchBoxOnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs eventArgs)
    {
        if (eventArgs.Reason is not AutoSuggestionBoxTextChangeReason.UserInput)
        {
            return;
        }

        timer.Stop();
        timer.Start();
    }

    private void SearchBoxOnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs eventArgs)
    {
        if (viewModel.Suggestions.Count <= 0)
        {
            return;
        }

        viewModel.SelectCommand.Execute((string?) eventArgs.ChosenSuggestion);
        sender.Text = string.Empty;
    }

    private void SearchInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs eventArgs)
    {
        SearchBox.Focus(FocusState.Programmatic);
    }

    private void NavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs eventArgs)
    {
        viewModel.NavigateCommand.Execute((ItemViewModel?) eventArgs.SelectedItemContainer?.DataContext);
    }

    private void FrameLoaded(object sender, RoutedEventArgs eventArgs)
    {
        if (Bootstrapper.Services.GetRequiredService<INavigationService>() is NavigationService navigationService)
        {
            navigationService.Frame = Frame;
        }

        viewModel.InitializeCommand.Execute(null);
    }
}