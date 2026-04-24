// Converters/DirectionColorConverter.cs
using System.Globalization;

namespace CheltuieliApp.Converters;

public class DirectionColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var direction = value?.ToString();

        return direction == "Credit" ? Colors.Green : Colors.Red;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}