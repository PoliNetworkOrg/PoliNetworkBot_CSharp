using System;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class DateTimeClass
    {
        internal static DateTime? GetUntilDate(string[] time)
        {
            if (time == null)
                return null;

            if (time.Length < 3)
            {
                return null;
            }

            if (string.IsNullOrEmpty(time[1]) || string.IsNullOrEmpty(time[2]))
                return null;

            int? number;
            try
            {
                number = Convert.ToInt32(time[1]);
            }
            catch
            {
                return null;
            }

            int? how_may_seconds = GetHowManySeconds(time[2]);

            if (number == null || how_may_seconds == null)
                return null;

            int time_seconds_elapsed = number.Value * how_may_seconds.Value;

            return DateTime.Now.AddSeconds(time_seconds_elapsed);
        }

        private static int? GetHowManySeconds(string v)
        {
            if (string.IsNullOrEmpty(v))
                return null;

            v = v.ToLower();

            switch (v)
            {
                case "y":
                case "year":
                case "years":
                case "anni":
                case "anno":
                    {
                        return 60 * 60 * 24 * 365;
                    }

                case "stagioni":
                case "stagione":
                    {
                        return 60 * 60 * 24 * 91;
                    }

                case "mo":
                case "month":
                case "months":
                case "mesi":
                case "mese":
                    {
                        return 60 * 60 * 24 * 30;
                    }

                case "w":
                case "week":
                case "weeks":
                case "settimane":
                case "settimana":
                    {
                        return 60 * 60 * 24 * 7;
                    }

                case "d":
                case "day":
                case "days":
                case "giorni":
                case "giorno":
                    {
                        return 60 * 60 * 24;
                    }

                case "h":
                case "hour":
                case "hours":
                case "ore":
                case "ora":
                    {
                        return 60 * 60;
                    }

                case "m":
                case "min":
                case "mins":
                case "minute":
                case "minutes":
                case "minuti":
                case "minuto":
                    {
                        return 60;
                    }

                default:
                    {
                        return 1;
                    }
            }
        }

        internal static string NowAsStringAmericanFormat()
        {
            var dt = DateTime.Now;
            return String.Format("{0:s}", dt) + ":" + dt.Millisecond.ToString().PadLeft(3, '0');
        }
    }
}