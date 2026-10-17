using System;
using Microsoft.UI.Xaml.Data;
using Siraj.Features.Prayers.Calculation;

namespace Siraj.Features.Settings.Converters;

internal sealed partial class MethodToIndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (int) (Method) value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return (Method) (int) value;
    }
}