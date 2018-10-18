using System;
using System.Globalization;
using System.Windows.Data;

namespace BrownianMotion_WPF.Converters
{
    class ButtonNameConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToBoolean(value) ? "Stop" : "Start";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}
