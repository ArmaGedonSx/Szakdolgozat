// Ez a konverter egy bool értéket alakít át Brush típusra.
using System;
using System.Windows.Data;
using System.Windows.Media;

namespace SocketChat
{
    public class BooleanToBrushConverter : IValueConverter
    {
        // Két különböző Brush a true és false értékekhez.
        private readonly Brush bTrue = new SolidColorBrush(Color.FromArgb(255, 192, 255, 192));
        private readonly Brush bFalse = new SolidColorBrush(Color.FromArgb(255, 255, 192, 192));

        // Convert metódus: bool értéket alakít át Brushra.
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value)
                {
                    return this.bTrue;
                }
            }

            return this.bFalse;
        }

        // ConvertBack metódus: Brush értéket alakít vissza bool-ra.
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (Brush)value == this.bTrue;
        }
    }
}
