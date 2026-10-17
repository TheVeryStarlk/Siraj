using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Serilog;

namespace Siraj.Features.Settings;

internal sealed class StartupService
{
    public async Task<bool> TryToggle(bool value)
    {
        Log.Debug("Updating startup task to {StartupEnabled}", value);

        var task = await StartupTask.GetAsync("SirajStartupTask");

        if (value)
        {
            var result = await task.RequestEnableAsync();

            Log.Debug("Startup task enable request returned {StartupState}", result);

            return result is StartupTaskState.Enabled;
        }

        task.Disable();

        Log.Debug("Disabled startup task");

        return true;
    }
}