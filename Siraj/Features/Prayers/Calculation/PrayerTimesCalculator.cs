using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Siraj.Features.Prayers.Calculation;

internal sealed class PrayerTimesCalculator(PrayerTimesCalculatorOptions calculatorOptions)
{
    public FrozenDictionary<PrayerKind, DateTimeOffset> Calculate(DateTimeOffset dateTimeOffset, double latitude, double longitude)
    {
        var context = new CalculationContext(
            dateTimeOffset,
            latitude,
            longitude,
            calculatorOptions.FajrAngle,
            calculatorOptions.IshaAngle,
            calculatorOptions.IshaOffset);

        return context.Calculate();
    }
}

file sealed class CalculationContext(
    DateTimeOffset dateTimeOffset,
    double latitude,
    double longitude,
    double fajrAngle,
    double ishaAngle,
    TimeSpan? ishaOffset)
{
    private readonly DateTimeOffset universal = new(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, 0, 0, 0, TimeSpan.Zero);

    public FrozenDictionary<PrayerKind, DateTimeOffset> Calculate()
    {
        var times = new Dictionary<PrayerKind, double>
        {
            { PrayerKind.Fajr, 5 },
            { PrayerKind.Dhuhr, 12 },
            { PrayerKind.Asr, 13 },
            { PrayerKind.Maghrib, 18 },
            { PrayerKind.Isha, 18 }
        };

        times[PrayerKind.Fajr] = AngleTime(fajrAngle, times[PrayerKind.Fajr], -1);
        times[PrayerKind.Dhuhr] = MidDay(times[PrayerKind.Dhuhr]);
        times[PrayerKind.Asr] = AngleTime(AsrAngle(times[PrayerKind.Asr]), times[PrayerKind.Asr]);
        times[PrayerKind.Maghrib] = AngleTime(0.833, times[PrayerKind.Maghrib]);
        times[PrayerKind.Isha] = AngleTime(ishaAngle, times[PrayerKind.Isha]);

        if (ishaOffset.HasValue)
        {
            times[PrayerKind.Isha] = times[PrayerKind.Maghrib] + ishaOffset.Value.TotalMinutes / 60D;
        }

        var result = new Dictionary<PrayerKind, DateTimeOffset>();

        foreach (var pair in times)
        {
            var time = pair.Value - longitude / 15D;
            result[pair.Key] = universal.AddHours(time);
        }

        return result.ToFrozenDictionary();
    }

    private (double Declination, double Equation) SunPosition(double time)
    {
        var epoch = (universal - new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalDays - 1 + time / 24D - longitude / 360D;

        var meanAnomaly = Math.Modulo(357.529 + 0.98560028 * epoch, 360);
        var meanLongitude = Math.Modulo(280.459 + 0.98564736 * epoch, 360);

        var eclipticLongitude = Math.Modulo(
            meanLongitude + 1.915 * Math.Sin(Math.DegreesToRadian(meanAnomaly))
                          + 0.020 * Math.Sin(Math.DegreesToRadian(2 * meanAnomaly)),
            360);

        var obliquity = 23.439 - 0.00000036 * epoch;

        var degrees = Math.RadianToDegrees(Math.Atan2(
            Math.Cos(Math.DegreesToRadian(obliquity)) * Math.Sin(Math.DegreesToRadian(eclipticLongitude)),
            Math.Cos(Math.DegreesToRadian(eclipticLongitude))));

        var rightAscension = Math.Modulo(degrees / 15D, 24);

        var declination = Math.RadianToDegrees(Math.Asin(Math.Sin(Math.DegreesToRadian(obliquity)) * Math.Sin(Math.DegreesToRadian(eclipticLongitude))));
        var equation = meanLongitude / 15D - rightAscension;

        return (declination, equation);
    }

    private double MidDay(double time)
    {
        var (_, equation) = SunPosition(time);
        return Math.Modulo(12 - equation, 24);
    }

    private double AngleTime(double angle, double time, int direction = 1)
    {
        var (declination, _) = SunPosition(time);

        var numerator = -Math.Sin(Math.DegreesToRadian(angle)) - Math.Sin(Math.DegreesToRadian(latitude)) * Math.Sin(Math.DegreesToRadian(declination));
        var denominator = Math.Cos(Math.DegreesToRadian(latitude)) * Math.Cos(Math.DegreesToRadian(declination));
        var hourAngle = Math.RadianToDegrees(Math.Acos(numerator / denominator)) / 15D;

        return MidDay(time) + hourAngle * direction;
    }

    private double AsrAngle(double time)
    {
        var (declination, _) = SunPosition(time);

        var radian = Math.DegreesToRadian(Math.Abs(latitude - declination));
        var tan = 1 + Math.Tan(radian);
        var atan = Math.Atan(1 / tan);
        var angle = Math.RadianToDegrees(atan);

        return -angle;
    }
}