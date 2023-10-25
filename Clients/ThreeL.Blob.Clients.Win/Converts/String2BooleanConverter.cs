using System;
using System.Globalization;
using System.Windows.Data;

namespace ThreeL.Blob.Clients.Win.Converts
{
    public class String2BooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var data  = value as string;
            if (string.IsNullOrEmpty(data))
                return false;

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
