using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using WinUIEx;
using Serilog;

namespace Siraj.Features.Shell;

internal sealed class WindowService
{
    private ShellView? window;

    public void Open()
    {
        Log.Debug("Showing the shell window");

        window ??= Bootstrapper.Services.GetRequiredService<ShellView>();

        window.WindowState = WindowState.Normal;
        window.BringToFront();

        window.AppWindow.Show();
    }

    public void Exit()
    {
        Log.Information("Closing Siraj");
        Application.Current.Exit();
    }
}