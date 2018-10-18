using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BrownianMotion_WPF.Converters
{
    class BallImageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var str = System.Convert.ToString(parameter);
                Uri uri;
                if (System.Convert.ToBoolean(values[0]))
                {
                    uri = System.Convert.ToBoolean(values[1]) 
                        ? new Uri($"pack://application:,,,/Images/{str}_Selected_Enabled.png") 
                        : new Uri($"pack://application:,,,/Images/{str}_Enabled1.png");
                }
                else
                if (System.Convert.ToBoolean(values[1]))
                {
                    uri = new Uri($"pack://application:,,,/Images/{str}_Selected_Disabled.png");
                }
                else
                {
                    uri = new Uri($"pack://application:,,,/Images/{str}_Disabled1.png");
                }
                BitmapImage source = new BitmapImage(uri);
                return source;
            }
            catch (Exception e)
            {
                return null;
            }
        }



        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
