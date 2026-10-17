using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Serilog;

namespace Siraj.Features.Prayers;

internal sealed class NotificationService
{
    public void Show(string title, string message)
    {
        Log.Information("Showing notification {Title}", title);

        var notification = new AppNotificationBuilder()
            .AddText(title)
            .AddText(message)
            .BuildNotification();

        AppNotificationManager.Default.Show(notification);
    }
}