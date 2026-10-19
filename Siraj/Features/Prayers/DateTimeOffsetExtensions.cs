using System;

namespace Siraj.Features.Prayers;

internal static class DateTimeOffsetExtensions
{
    public static DateTimeOffset TruncateToMinute(this DateTimeOffset dateTimeOffset)
    {
        return new DateTimeOffset(dateTimeOffset.Ticks - dateTimeOffset.Ticks % TimeSpan.TicksPerMinute, dateTimeOffset.Offset);
    }
}
