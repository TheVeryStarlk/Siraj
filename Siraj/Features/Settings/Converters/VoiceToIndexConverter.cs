using System;
using Microsoft.UI.Xaml.Data;
using Siraj.Features.Prayers.Voices;

namespace Siraj.Features.Settings.Converters;

internal sealed partial class VoiceToIndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (int) (Voice) value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return (Voice) (int) value;
    }
}