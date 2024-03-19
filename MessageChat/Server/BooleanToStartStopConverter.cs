// Ez a konverter két bool értéket alakít át ButtonText-re.
using System;
using System.Globalization;
using System.Windows.Data;

namespace SocketChat
{
    public class BooleanToStartStopConverter : IMultiValueConverter
    {
        // Konstans szövegek a buttontext-hez.
        private const string srvTrue = "Stop";
        private const string srvFalse = "Start";
        private const string clntTrue = "Disconnect";
        private const string clntFalse = "Connect";
        private const string error = "?";

        // Convert metódus: két bool értéket alakít át buttontext-re.
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isServer = (bool)value[0];
            bool isActive = (bool)value[1];

            if (isServer && isActive)
            {
                return srvTrue;
            }

            if (isServer && !isActive)
            {
                return srvFalse;
            }

            if (!isServer && isActive)
            {
                return clntTrue;
            }

            if (!isServer && !isActive)
            {
                return clntFalse;
            }
            else
            {
                return error;
            }
        }

        // ConvertBack metódus implementációja a felületi szabványnak.
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
