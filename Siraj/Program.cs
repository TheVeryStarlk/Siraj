using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Serilog;
using WinRT;

namespace Siraj;

internal static partial class Program
{
    [STAThread]
    public static int Main()
    {
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Siraj");

        var logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(directory, ".log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

        Log.Logger = logger.CreateLogger();

        try
        {
            Log.Information("Starting Siraj");

            ComWrappersSupport.InitializeComWrappers();

            if (IsRedirect())
            {
                Log.Information("Redirected activation to the existing Siraj instance");
                return 0;
            }

            Application.Start(callback =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());

                SynchronizationContext.SetSynchronizationContext(context);

                _ = new App();
            });

            Log.Information("Siraj exited");
            return 0;
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "Siraj terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static bool IsRedirect()
    {
        var instance = AppInstance.FindOrRegisterForKey(nameof(Siraj));
        var redirect = !instance.IsCurrent;

        if (redirect)
        {
            RedirectActivation(AppInstance.GetCurrent().GetActivatedEventArgs(), instance);
        }

        return redirect;
    }

    private static void RedirectActivation(AppActivationArguments args, AppInstance instance)
    {
        var handle = CreateEvent(nint.Zero, true, false, null);

        Task.Run(() =>
        {
            instance.RedirectActivationToAsync(args).AsTask().Wait();
            SetEvent(handle);
        });

        CoWaitForMultipleObjects(0, 0xFFFFFFFF, 1, [handle], out _);
        SetForegroundWindow(Process.GetProcessById((int) instance.ProcessId).MainWindowHandle);
    }

    [LibraryImport("kernel32.dll", EntryPoint = "CreateEventW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint CreateEvent(nint lpEventAttributes, [MarshalAs(UnmanagedType.Bool)] bool bManualReset, [MarshalAs(UnmanagedType.Bool)] bool bInitialState, string? lpName);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetEvent(nint hEvent);

    [LibraryImport("ole32.dll")]
    private static partial uint CoWaitForMultipleObjects(uint dwFlags, uint dwMilliseconds, ulong nHandles, nint[] pHandles, out uint dwIndex);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(nint hWnd);
}