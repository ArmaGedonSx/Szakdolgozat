// Ez a konverter egy bool értéket invertál.
using System;
using System.Windows.Data;

namespace SocketChat
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BooleanToInverseConverter : IValueConverter
    {
        // Convert metódus: bool értéket invertál.
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        // ConvertBack metódus: nem támogatott (+ nincs még implementálva, ezért egy exceptiont dobok.
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
