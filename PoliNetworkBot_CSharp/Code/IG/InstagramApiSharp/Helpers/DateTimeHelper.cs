#region

using System;

#endregion

namespace InstagramApiSharp.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static double GetUnixTimestampMilliseconds(DateTime dt)
        {
            var span = dt - UnixEpoch;
            return span.TotalMilliseconds;
        }

        public static DateTime UnixTimestampToDateTime(double unixTime)
        {
            try
            {
                var time = (long)unixTime;
                return time.FromUnixTimeSeconds();
            }
            catch
            {
            }

            return DateTime.Now;
        }

        public static DateTime UnixTimestampToDateTime(string unixTime)
        {
            if (unixTime.Length > 10) return UnixTimestampMilisecondsToDateTime(unixTime);
            var time = (long)Convert.ToDouble(unixTime);
            return time.FromUnixTimeSeconds();

        }

        public static DateTime UnixTimestampMilisecondsToDateTime(string unixTime)
        {
            try
            {
                var time = (long)Convert.ToDouble(unixTime) / 1000000;
                return time.FromUnixTimeSeconds();
            }
            catch
            {
            }

            return DateTime.Now;
        }

        public static DateTime FromUnixTimeSeconds(this long unixTime)
        {
            try
            {
                return UnixEpoch.AddSeconds(unixTime);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public static DateTime FromUnixTimeMiliSeconds(this long unixTime)
        {
            try
            {
                return UnixEpoch.AddMilliseconds(unixTime);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public static long ToUnixTime(this DateTime date)
        {
            try
            {
                return Convert.ToInt64((date - UnixEpoch).TotalSeconds);
            }
            catch
            {
                return 0;
            }
        }

        public static long ToUnixTimeMiliSeconds(this DateTime date)
        {
            try
            {
                return Convert.ToInt64((date - UnixEpoch).TotalMilliseconds);
            }
            catch
            {
                return 0;
            }
        }

        public static long GetUnixTimestampSeconds()
        {
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            return (long)timeSpan.TotalSeconds;
        }
    }
}