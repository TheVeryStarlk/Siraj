using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

namespace Siraj.Features.Settings;

internal sealed class LauncherService
{
    public async Task OpenLogsAsync()
    {
        var roaming = await ApplicationData.Current.LocalCacheFolder.GetFolderAsync("Roaming");
        var siraj = await roaming.GetFolderAsync("Siraj");
        var files = await siraj.GetFilesAsync();
        var file = files.FirstOrDefault(file => string.Equals(file.DisplayName, DateTimeOffset.UtcNow.ToString("yyyyMMdd")));

        if (file is null)
        {
            return;
        }

        await Launcher.LaunchFileAsync(file);
    }
}