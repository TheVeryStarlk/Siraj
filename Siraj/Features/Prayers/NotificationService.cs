using System;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Serilog;

namespace Siraj.Features.Prayers;

internal sealed class NotificationService
{
    public void Show(string title, string message, DateTimeOffset? expiration = null)
    {
        Log.Information("Showing notification {Title}", title);

        var notification = new AppNotificationBuilder()
            .AddText(title)
            .AddText(message)
            .BuildNotification();

        if (expiration.HasValue)
        {
            notification.Expiration = expiration.Value;
        }

        AppNotificationManager.Default.Show(notification);
    }
}