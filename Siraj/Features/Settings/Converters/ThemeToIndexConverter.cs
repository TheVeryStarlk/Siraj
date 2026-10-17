using System;
using Microsoft.UI.Xaml.Data;

namespace Siraj.Features.Settings.Converters;

internal sealed partial class ThemeToIndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (int) (Theme) value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return (Theme) (int) value;
    }
}