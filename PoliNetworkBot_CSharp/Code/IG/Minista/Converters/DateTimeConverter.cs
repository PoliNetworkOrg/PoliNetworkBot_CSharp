#region

using System;
using Windows.UI.Xaml.Data;

#endregion

namespace Minista.Converters
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var v = DateTime.UtcNow.Subtract(System.Convert.ToDateTime(value));
            if (v.TotalHours < 1)
            {
                if (v.TotalSeconds >= 1)
                {
                    if (v.TotalSeconds > 59) return $"{System.Convert.ToInt32(v.TotalMinutes)}min";

                    if (v.Seconds > 10)
                        return $"{System.Convert.ToInt32(v.Seconds)}s";
                    return "Just now";
                }

                return "Just now";
            }

            if (v.TotalHours < 24)
                return $"{System.Convert.ToInt32(v.TotalHours)}h";
            if (v.TotalDays <= 7)
                return $"{System.Convert.ToInt32(v.TotalDays)}d";
            if (System.Convert.ToInt32(v.TotalDays / 7) < 4)
                return $"{System.Convert.ToInt32(v.TotalDays / 7)}w";
            if (System.Convert.ToInt32(System.Convert.ToInt32(v.TotalDays / 7) / 4) < 12)
                return $"{System.Convert.ToInt32(v.TotalDays / 7)}w";
            //return $"{System.Convert.ToInt32(System.Convert.ToInt32(v.TotalDays / 7) / 4)}m";
            return
                $"{System.Convert.ToInt32(System.Convert.ToInt32(System.Convert.ToInt32(v.TotalDays / 7) / 4) / 12)}y";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public object Convert(object value)
        {
            return Convert(value, value.GetType(), null, string.Empty);
        }
    }
}